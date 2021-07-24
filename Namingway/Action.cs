using System.Diagnostics.CodeAnalysis;

namespace Namingway {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal enum Action : uint {
        // BLM
        FireII = 147,
        FireIII = 152,
        FireIV = 3_577,
        BlizzardII = 146,
        BlizzardIII = 154,
        BlizzardIV = 3_576,
        ThunderII = 7_447,
        ThunderIII = 153,
        ThunderIV = 7_420,

        // WHM
        StoneII = 127,
        StoneIII = 3_568,
        StoneIV = 7_431,
        AeroII = 132,
        CureII = 135,
        CureIII = 131,
        MedicaII = 133,

        // RDM
        JoltII = 7_524,
        VerthunderII = 16_524,
        VeraeroII = 16_525,

        // AST
        BeneficII = 3_610,
        CombustII = 3_608,
        CombustIII = 16_554,
        MaleficII = 3_598,
        MaleficIII = 7_442,
        MaleficIV = 16_555,

        // SCH
        SchBioII = 17_865,
        SchRuinII = 17_870,
        BroilII = 7_435,
        BroilIII = 16_541,

        // SMN
        SmnBioII = 178,
        BioIII = 7_424,
        MiasmaIII = 7_425,
        SmnRuinII = 172,
        RuinIII = 3_579,
        RuinIV = 7_426,

        // Bozja
        LostCureII = 20_727,
        LostCureIII = 20_728,
        LostCureIV = 20_729,
    }
}
