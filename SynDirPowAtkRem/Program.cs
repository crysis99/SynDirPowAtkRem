using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Plugins;

namespace SynDirPowAtkRem
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "SynDirPowAtkRem.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            var mgefForwardLink = new FormLink<IMagicEffectGetter>(FormKey.Factory("000801:Keytrace.esp"));
            var mgefLeftLink = new FormLink<IMagicEffectGetter>(FormKey.Factory("000802:Keytrace.esp"));
            var mgefBackwardLink = new FormLink<IMagicEffectGetter>(FormKey.Factory("000803:Keytrace.esp"));
            var mgefRightLink = new FormLink<IMagicEffectGetter>(FormKey.Factory("000804:Keytrace.esp"));

            var itemLink = new FormLink<IKeywordGetter>(FormKey.Factory("0914E7:Skyrim.esm"));
            if (!itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var paTypeSide))
            {
                throw new ArgumentException();
            }
            itemLink = new FormLink<IKeywordGetter>(FormKey.Factory("0914E6:Skyrim.esm"));
            if (!itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var paTypeForward))
            {
                throw new ArgumentException();
            }
            itemLink = new FormLink<IKeywordGetter>(FormKey.Factory("0914E8:Skyrim.esm"));
            if (!itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var paTypeBack))
            {
                throw new ArgumentException();
            }
            itemLink = new FormLink<IKeywordGetter>(FormKey.Factory("0914E5:Skyrim.esm"));
            if (!itemLink.TryResolve<IKeywordGetter>(state.LinkCache, out var paTypeStanding))
            {
                throw new ArgumentException();
            }

            //Forward
            ConditionFloat conForward = new ConditionFloat();
            HasMagicEffectConditionData conDataForward = new HasMagicEffectConditionData();
            conForward.CompareOperator = CompareOperator.EqualTo;
            conDataForward.RunOnType = Condition.RunOnType.Subject;
            conDataForward.Unknown3 = -1;
            conDataForward.UseAliases=false;
            conDataForward.UsePackageData=false;
            conDataForward.SecondUnusedIntParameter = 0;
            conDataForward.MagicEffect.Link.SetTo(mgefForwardLink);
            conForward.Data = conDataForward;

            //Left
            ConditionFloat conLeft = new ConditionFloat();
            HasMagicEffectConditionData conDataLeft = new HasMagicEffectConditionData();
            conLeft.CompareOperator = CompareOperator.EqualTo;
            conLeft.Flags = Condition.Flag.OR;
            conDataLeft.RunOnType = Condition.RunOnType.Subject;
            conDataLeft.Unknown3 = -1;
            conDataLeft.UseAliases=false;
            conDataLeft.UsePackageData=false;
            conDataForward.SecondUnusedIntParameter = 0;
            conDataLeft.MagicEffect.Link.SetTo(mgefLeftLink);
            conLeft.Data = conDataLeft;

            //Backward
            ConditionFloat conBackward = new ConditionFloat();
            HasMagicEffectConditionData conDataBackward = new HasMagicEffectConditionData();
            conBackward.CompareOperator = CompareOperator.EqualTo;
            conDataBackward.RunOnType = Condition.RunOnType.Subject;
            conDataBackward.Unknown3 = -1;
            conDataBackward.UseAliases=false;
            conDataBackward.UsePackageData=false;
            conDataForward.SecondUnusedIntParameter = 0;
            conDataBackward.MagicEffect.Link.SetTo(mgefBackwardLink);
            conBackward.Data = conDataBackward;

            //Right
            ConditionFloat conRight = new ConditionFloat();
            HasMagicEffectConditionData conDataRight = new HasMagicEffectConditionData();
            conRight.CompareOperator = CompareOperator.EqualTo;
            conDataRight.RunOnType = Condition.RunOnType.Subject;
            conDataRight.Unknown3 = -1;
            conDataRight.UseAliases=false;
            conDataRight.UsePackageData=false;
            conDataForward.SecondUnusedIntParameter = 0;
            conDataRight.MagicEffect.Link.SetTo(mgefRightLink);
            conRight.Data = conDataRight;

            ConditionFloat conNotLeft = conLeft;
            conNotLeft.CompareOperator = CompareOperator.NotEqualTo;
            ConditionFloat conNotRight = conRight;
            conNotRight.CompareOperator = CompareOperator.NotEqualTo;
            conNotRight.Flags = Condition.Flag.OR;
            ConditionFloat conNotFront = conForward;
            conNotFront.CompareOperator = CompareOperator.NotEqualTo;
            conRight.Flags = Condition.Flag.OR;
            ConditionFloat conNotBack = conBackward;
            conNotBack.CompareOperator = CompareOperator.NotEqualTo;
            
            foreach (IPerkGetter perk in state.LoadOrder.PriorityOrder.OnlyEnabled().Perk().WinningOverrides())
            {
                if(perk.Name==null||perk.EditorID==null){continue;}
                string perkNameStr = perk.Name.String ?? "blank";
                if(perkNameStr.Contains("NPC",StringComparison.InvariantCultureIgnoreCase)||perk.EditorID.Contains("NPC",StringComparison.InvariantCultureIgnoreCase)){continue;}
                if(perk.Effects.Count>0)
                {
                    for(int i = 0;i<perk.Effects.Count;i++)
                    {
                        if(perk.Effects[i].Conditions.Count>0)
                        {
                            for(int j = 0;j<perk.Effects[i].Conditions.Count;j++)
                            {
                                if(perk.Effects[i].Conditions[j].Conditions.Count>0)
                                {
                                    for(int k = 0;k<perk.Effects[i].Conditions[j].Conditions.Count;k++)
                                    {
                                        if(perk.Effects[i].Conditions[j].Conditions[k].Data.ToString()=="Mutagen.Bethesda.Skyrim.IsAttackTypeConditionData")
                                        {
                                            IsAttackTypeConditionData currCon = (IsAttackTypeConditionData) perk.Effects[i].Conditions[j].Conditions[k].Data;
                                            string compStr = currCon.Keyword.Link.Resolve<IKeywordGetter>(state.LinkCache).EditorID ?? "";
                                            if(compStr==paTypeForward.EditorID) 
                                            {
                                                var perkOverride = state.PatchMod.Perks.GetOrAddAsOverride(perk);
                                                perkOverride.Effects[i].Conditions[j].Conditions[k] = conForward;
                                            }
                                            else if(compStr==paTypeBack.EditorID) 
                                            {
                                                var perkOverride = state.PatchMod.Perks.GetOrAddAsOverride(perk);
                                                perkOverride.Effects[i].Conditions[j].Conditions[k] = conBackward;
                                            }
                                            else if(compStr==paTypeSide.EditorID) 
                                            {
                                                var perkOverride = state.PatchMod.Perks.GetOrAddAsOverride(perk);
                                                perkOverride.Effects[i].Conditions[j].Conditions[k] = conLeft;
                                                perkOverride.Effects[i].Conditions[j].Conditions.Insert(k+1,conRight);
                                            }
                                            else if(compStr==paTypeStanding.EditorID)
                                            {
                                                var perkOverride = state.PatchMod.Perks.GetOrAddAsOverride(perk);
                                                perkOverride.Effects[i].Conditions[j].Conditions[k] = conNotLeft;
                                                perkOverride.Effects[i].Conditions[j].Conditions.Insert(k+1,conNotRight);
                                                perkOverride.Effects[i].Conditions[j].Conditions.Insert(k+2,conNotFront);
                                                perkOverride.Effects[i].Conditions[j].Conditions.Insert(k+3,conNotBack);
                                            }
                                        }
                                
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
        }
    }
}
// PowerAttackTypeStanding [KYWD:000914E5]
// PowerAttackTypeSide [KYWD:000914E7]
// PowerAttackTypeForward [KYWD:000914E6]
// PowerAttackTypeBack [KYWD:000914E8]

// ForwardMGEF [MGEF:FE000801]
// LeftMGEF [MGEF:FE000802]
// BackwardMGEF [MGEF:FE000803]
// RightMGEF [MGEF:FE000804]

// Condition =>
// [
//     CompareOperator => EqualTo
//     Flags => 0
//     Unknown1 => 000000
//     Unknown2 => 0
//     Data (HasMagicEffectConditionData) =>
//     [
//         RunOnType => Subject
//         Reference => Null
//         Unknown3 => -1
//         UseAliases => False
//         UsePackageData => False
//         MagicEffect => Mutagen.Bethesda.Plugins.FormLinkOrIndex`1[Mutagen.Bethesda.Skyrim.IMagicEffectGetter]
//         SecondUnusedIntParameter => 0
//     ]
// ]