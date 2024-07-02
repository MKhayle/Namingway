using System;
using System.Collections.Generic;

namespace Namingway;

internal static class DefaultPacks {
    private static readonly Pack SpellSuffixWhm = new("Spell Suffixes (WHM)") {
        Id = new Guid("9C46FB72-824E-476C-8649-8ECAEFD6C280"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.StoneII] = "Stonera",
            [(uint) Action.StoneIII] = "Stonega",
            [(uint) Action.StoneIV] = "Stoneja",
            [(uint) Action.AeroII] = "Aerora",
            [(uint) Action.CureII] = "Cura",
            [(uint) Action.CureIII] = "Curaga",
            [(uint) Action.MedicaII] = "Medicara",
            [(uint) Action.MedicaIII] = "Medicaga",
            [(uint) Action.GlareIII] = "Glarega",
            [(uint) Action.HolyIII] = "Holyga",
        },
        Statuses = new Dictionary<uint, string> {
            [(uint) Status.AeroII] = "Aerora",
            [(uint) Status.MedicaII] = "Medicara",
            [(uint) Status.MedicaIII] = "Medicaga",
        },
    };

    private static readonly Pack SpellSuffixBlm = new("Spell Suffixes (BLM)") {
        Id = new Guid("3E12EE15-F7DC-448A-90BF-4734EA9CCA17"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.FireII] = "Fira",
            [(uint) Action.FireIII] = "Firaga",
            [(uint) Action.FireIV] = "Firaja",
            [(uint) Action.BlizzardII] = "Blizzara",
            [(uint) Action.BlizzardIII] = "Blizzaga",
            [(uint) Action.BlizzardIV] = "Blizzaja",
            [(uint) Action.ThunderII] = "Thundara",
            [(uint) Action.ThunderIII] = "Thundaga",
            [(uint) Action.ThunderIV] = "Thundaja",
            [(uint) Action.HighFireII] = "High Fira",
            [(uint) Action.HighBlizzardII] = "High Blizzara",
            [(uint) Action.HighThunderII] = "High Thundara",
        },
        Statuses = new Dictionary<uint, string> {
            [(uint) Status.ThunderII] = "Thundara",
            [(uint) Status.ThunderIII] = "Thundaga",
            [(uint) Status.ThunderIV] = "Thundaja",
        },
    };

    private static readonly Pack SpellSuffixAst = new("Spell Suffixes (AST)") {
        Id = new Guid("F8EE5346-5254-4B99-8392-65FBC6DAD952"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.BeneficII] = "Benefira",
            [(uint) Action.CombustII] = "Combustra",
            [(uint) Action.CombustIII] = "Combustga",
            [(uint) Action.MaleficII] = "Malefira",
            [(uint) Action.MaleficIII] = "Malefiga",
            [(uint) Action.MaleficIV] = "Malefija",
            [(uint) Action.GravityII] = "Gravira",
        },
        Statuses = new Dictionary<uint, string> {
            [(uint) Status.CombustII] = "Combustra",
            [(uint) Status.CombustIII] = "Combustga",
            [(uint) Status.EnhancedBeneficII] = "Enhanced Benefira",
        },
    };

    private static readonly Pack SpellSuffixSch = new("Spell Suffixes (SCH)") {
        Id = new Guid("D5FEF979-493B-41A8-A4BC-3FE234C490F3"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.SchBioII] = "Biora",
            [(uint) Action.SchRuinII] = "Ruinra",
            [(uint) Action.BroilII] = "Broilra",
            [(uint) Action.BroilIII] = "Broilga",
            [(uint) Action.BroilIV] = "Broilja",
        },
        Statuses = new Dictionary<uint, string> {
            [(uint) Status.BioII] = "Biora",
        },
    };

    private static readonly Pack SpellSuffixSch2 = new("Spell Suffixes (SCH 2)") {
        Id = new Guid("0E92ACD6-4EAF-4857-9EDE-F76F35F1D415"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.ArtOfWar] = "Scourge",
            [(uint) Action.ArtOfWarII] = "Scoura",
        },
    };

    private static readonly Pack SpellSuffixSge = new("Spell Suffixes (SGE)") {
        Id = new Guid("4DE99035-4469-4589-84F7-717815F6DDCA"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.DosisII] = "Dosira",
            [(uint) Action.DosisIII] = "Dosiga",
            [(uint) Action.EukrasianDosisII] = "Eukrasian Dosira",
            [(uint) Action.EukrasianDosisIII] = "Eukrasian Dosiga",
            [(uint) Action.PhlegmaII] = "Phlegmara",
            [(uint) Action.PhlegmaIII] = "Phlegmaga",
            [(uint) Action.DyskrasiaII] = "Dyskrasira",
            [(uint) Action.ToxikonII] = "Toxikira",
            [(uint) Action.PhysisII] = "Physira",
            [(uint) Action.EukrasianPrognosisII] = "Eukrasian Prognosira",
        },
        Statuses = new Dictionary<uint, string> {
            [(uint) Status.EukrasianDosisII] = "Eukrasian Dosira",
            [(uint) Status.EukrasianDosisIII] = "Eukrasian Dosiga",
            [(uint) Status.PhysisII] = "Physira",
        },
    };

    private static readonly Pack SpellSuffixSmn = new("Spell Suffixes (SMN)") {
        Id = new Guid("C10324AE-0A08-4AD6-95B7-389570C34B12"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.SmnRuinII] = "Ruinra",
            [(uint) Action.RuinIII] = "Ruinga",
            [(uint) Action.RuinIV] = "Ruinja",
            [(uint) Action.RubyRuinII] = "Ruby Ruinra",
            [(uint) Action.RubyRuinIII] = "Ruby Ruinga",
            [(uint) Action.TopazRuinII] = "Topaz Ruinra",
            [(uint) Action.TopazRuinIII] = "Topaz Ruinga",
            [(uint) Action.EmeraldRuinII] = "Emerald Ruinra",
            [(uint) Action.EmeraldRuinIII] = "Emerald Ruinga",
        },
    };

    private static readonly Pack SpellSuffixRdm = new("Spell Suffixes (RDM)") {
        Id = new Guid("ECEC3695-52E7-4691-A996-ACA3BC9B88B9"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.JoltII] = "Joltra",
            [(uint) Action.JoltIII] = "Joltga",
            [(uint) Action.VerthunderII] = "Verthundara",
            [(uint) Action.VerthunderIII] = "Verthundaga",
            [(uint) Action.VeraeroII] = "Veraerora",
            [(uint) Action.VeraeroIII] = "Veraeroga",
        },
    };

    private static readonly Pack SpellSuffixPct = new("Spell Suffixes (PCT)") {
        Id = new Guid("CDA25E5D-8823-436B-96C6-5EEA83040268"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.Fire2InRed] = "Fira in Red",
            [(uint) Action.Aero2InGreen] = "Aerora in Green",
            [(uint) Action.Water2InBlue] = "Watera in Blue",
            [(uint) Action.Blizzard2InCyan] = "Blizzara in Cyan",
            [(uint) Action.Stone2InYellow] = "Stona in Yellow",
            [(uint) Action.Thunder2InMagenta] = "Thundara in Magenta",
        },
    };

    private static readonly Pack SpellSuffixBozja = new("Spell Suffixes (Bozja)") {
        Id = new Guid("DE1523BB-47B6-4415-8B62-7804C3595847"),
        Actions = new Dictionary<uint, string> {
            [(uint) Action.LostCureII] = "Lost Cura",
            [(uint) Action.LostCureIII] = "Lost Curaga",
            [(uint) Action.LostCureIV] = "Lost Curaja",
        },
    };

    internal static readonly Pack[] All = [
        SpellSuffixWhm,
        SpellSuffixAst,
        SpellSuffixSch,
        SpellSuffixSch2,
        SpellSuffixSge,
        SpellSuffixBlm,
        SpellSuffixSmn,
        SpellSuffixRdm,
        SpellSuffixPct,
        SpellSuffixBozja,
    ];
}
