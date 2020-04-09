namespace Adora
{
    /// <summary>
    ///     The base interface for Adora checks.
    /// </summary>
    public interface IAdoraCheck
    {
        /// <summary>
        ///     Returns the friendly name of the check, an instruction to the user about what this check entails.
        /// </summary>
        /// <param name="commandContext">The command context, passed in in-case it is needed.</param>
        /// <returns>A string representing the friendly name of the check.</returns>
        string GetDescription(AdoraCommandContext commandContext);
    }
}