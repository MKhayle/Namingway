using System.Diagnostics.CodeAnalysis;

namespace Namingway {
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal enum Action : uint {
        // BLM
        FireII = 147,
        FireIII = 152,
        FireIV = 3_577,
        BlizzardII = 25_793,
        BlizzardIII = 154,
        BlizzardIV = 3_576,
        ThunderII = 7_447,
        ThunderIII = 153,
        ThunderIV = 7_420,
        HighFireII = 25_794,
        HighBlizzardII = 25_795,

        // WHM
        StoneII = 127,
        StoneIII = 3_568,
        StoneIV = 7_431,
        AeroII = 132,
        CureII = 135,
        CureIII = 131,
        MedicaII = 133,
        GlareIII = 25_859,
        HolyIII = 25_860,

        // RDM
        JoltII = 7_524,
        VerthunderII = 16_524,
        VeraeroII = 16_525,
        VerthunderIII = 25_855,
        VeraeroIII = 25_856,

        // AST
        BeneficII = 3_610,
        CombustII = 3_608,
        CombustIII = 16_554,
        MaleficII = 3_598,
        MaleficIII = 7_442,
        MaleficIV = 16_555,
        GravityII = 25_872,

        // SCH
        SchBioII = 17_865,
        SchRuinII = 17_870,
        BroilII = 7_435,
        BroilIII = 16_541,
        BroilIV = 25_865,
        ArtOfWar = 16_539,
        ArtOfWarII = 25_866,

        // SMN
        SmnRuinII = 172,
        RuinIII = 3_579,
        RuinIV = 7_426,

        // Bozja
        LostCureII = 20_727,
        LostCureIII = 20_728,
        LostCureIV = 20_729,
    }
}
