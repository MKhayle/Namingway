using System.Diagnostics.CodeAnalysis;

namespace Namingway;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum Status : uint {
    // WHM
    AeroII = 144,
    MedicaII = 150,
    MedicaIII = 3_880,

    // BLM
    ThunderII = 162,
    ThunderIII = 163,
    ThunderIV = 1_210,

    // AST
    CombustII = 843,
    CombustIII = 1_881,
    EnhancedBeneficII = 815,

    // SGE
    EukrasianDosisII = 2_615,
    EukrasianDosisIII = 2_616,
    PhysisII = 2_620,

    // SMN
    BioII = 189, // also affects SCH
    BioIII = 1_214,
    MiasmaIII = 1_215,
}
