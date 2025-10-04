using Microsoft.Xrm.Sdk;

public static class EntityMergeExtensions
{
    /// <summary>
    /// Merge attributes from a pre-image into this entity,
    /// only filling fields that are not already present in post.
    /// </summary>
    public static void MergePreImage(this Entity post, Entity pre)
    {
        if (pre == null) return;
        foreach (var kvp in pre.Attributes)
        {
            if (!post.Attributes.ContainsKey(kvp.Key))
            {
                post[kvp.Key] = kvp.Value;
            }
        }
    }
}
