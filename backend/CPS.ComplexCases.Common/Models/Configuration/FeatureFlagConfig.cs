namespace CPS.ComplexCases.Common.Models.Configuration;

public class FeatureFlagConfig
{
    public const string SectionName = "FeatureFlags";

    /// <summary>
    /// Enables case_active_manage_materials table access on GetCase API.
    /// </summary>
    public bool ManageMaterials { get; set; } = false;

    // ----- Added for rollback test ----------------------
    public bool SimulateEndpointFailure { get; set; } = false;
    // ---------------------------------------------------
}