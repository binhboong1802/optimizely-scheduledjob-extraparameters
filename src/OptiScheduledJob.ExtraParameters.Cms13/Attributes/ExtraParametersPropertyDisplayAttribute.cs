namespace OptiScheduledJob.ExtraParameters.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExtraParametersPropertyDisplayAttribute: Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Dropdown options. Set as a constant string array on the attribute, e.g.
        /// <c>Options = new[] { "Low", "Medium", "High" }</c>. Each entry is used as both the
        /// option value and its display text.
        /// </summary>
        public string[] Options { get; set; }

    }
}
