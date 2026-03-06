using System;
using System.Collections.Generic;

public static class DefinitionReferenceValidator
{
    public static void ValidateSingleReference<THost>(
        IEnumerable<THost> hosts,
        Func<THost, string> hostAssetName,
        Func<THost, string> hostId,
        Func<THost, string> referenceId,
        string fieldName,
        Func<string, bool> targetExists,
        Action<string> reportError)
    {
        foreach (var host in hosts)
        {
            if (host == null)
                continue;

            var targetId = referenceId(host);
            if (string.IsNullOrWhiteSpace(targetId))
                continue;

            if (!targetExists(targetId))
                reportError(FormatMissingReferenceError(hostAssetName(host), hostId(host), fieldName, targetId));
        }
    }

    public static void ValidateReferenceList<THost>(
        IEnumerable<THost> hosts,
        Func<THost, string> hostAssetName,
        Func<THost, string> hostId,
        Func<THost, IEnumerable<string>> referenceIds,
        string fieldName,
        Func<string, bool> targetExists,
        Action<string> reportError)
    {
        foreach (var host in hosts)
        {
            if (host == null)
                continue;

            var ids = referenceIds(host);
            if (ids == null)
                continue;

            foreach (var targetId in ids)
            {
                if (string.IsNullOrWhiteSpace(targetId))
                    continue;

                if (!targetExists(targetId))
                    reportError(FormatMissingReferenceError(hostAssetName(host), hostId(host), fieldName, targetId));
            }
        }
    }

    public static void ValidateReferenceCollection<THost, TReference>(
        IEnumerable<THost> hosts,
        Func<THost, string> hostAssetName,
        Func<THost, string> hostId,
        Func<THost, IEnumerable<TReference>> references,
        Func<TReference, string> referenceId,
        string fieldName,
        Func<string, bool> targetExists,
        Action<string> reportError)
    {
        foreach (var host in hosts)
        {
            if (host == null)
                continue;

            var items = references(host);
            if (items == null)
                continue;

            foreach (var reference in items)
            {
                var targetId = referenceId(reference);
                if (string.IsNullOrWhiteSpace(targetId))
                    continue;

                if (!targetExists(targetId))
                    reportError(FormatMissingReferenceError(hostAssetName(host), hostId(host), fieldName, targetId));
            }
        }
    }

    private static string FormatMissingReferenceError(string hostAssetName, string hostId, string fieldName, string missingTargetId)
    {
        return $"[Validation] Asset '{hostAssetName}' (id: '{hostId}') field '{fieldName}' references missing target id '{missingTargetId}'.";
    }
}
