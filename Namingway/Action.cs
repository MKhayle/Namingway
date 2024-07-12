using System.Diagnostics.CodeAnalysis;

namespace Namingway;

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
    HighThunderII = 36_987,

    // WHM
    StoneII = 127,
    StoneIII = 3_568,
    StoneIV = 7_431,
    AeroII = 132,
    CureII = 135,
    CureIII = 131,
    MedicaII = 133,
    MedicaIII = 37_010,
    GlareIII = 25_859,
    HolyIII = 25_860,
    GlareIV = 37_009,

    // RDM
    JoltII = 7_524,
    JoltIII = 37_004,
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

    // SGE
    DosisII = 24_306,
    DosisIII = 24_312,
    EukrasianDosisII = 24_308,
    EukrasianDosisIII = 24_314,
    PhlegmaII = 24_307,
    PhlegmaIII = 24_313,
    DyskrasiaII = 24_315,
    ToxikonII = 24_316,
    PhysisII = 24_302,
    EukrasianPrognosisII = 37_034,

    // SMN
    SmnRuinII = 172,
    RuinIII = 3_579,
    RuinIV = 7_426,
    RubyRuinII = 25_811,
    RubyRuinIII = 25_817,
    TopazRuinII = 25_812,
    TopazRuinIII = 25_818,
    EmeraldRuinII = 25_813,
    EmeraldRuinIII = 25_819,

    // PCT
    Fire2InRed = 34_656,
    Aero2InGreen = 34_657,
    Water2InBlue = 34_658,
    Blizzard2InCyan = 34_659,
    Stone2InYellow = 34_660,
    Thunder2InMagenta = 34_661,

    // Bozja
    LostCureII = 20_727,
    LostCureIII = 20_728,
    LostCureIV = 20_729,
}
