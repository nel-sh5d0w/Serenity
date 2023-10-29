namespace Serenity.ComponentModel;

/// <summary>
/// Marks a row class so that its fields should be generated by Serenity.Pro.Coder
/// RowFieldsGenerator
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateFieldsAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateFieldsAttribute"/> class.
    /// </summary>
    public GenerateFieldsAttribute()
    {
    }
}