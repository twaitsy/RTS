using System;
using System.Collections.Generic;
using UnityEngine;

public static class StatModifierLinkValidator
{
    public static void ValidateStatModifierDefinitions(
        IEnumerable<StatModifierDefinition> statModifierDefinitions,
        Func<string, bool> statExists,
        Action<string> reportError)
    {
        foreach (var statModifierDefinition in statModifierDefinitions)
        {
            if (statModifierDefinition == null)
                continue;

            foreach (var modifier in statModifierDefinition.Modifiers)
            {
                if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                {
                    reportError($"[Validation] Asset '{statModifierDefinition.name}' (id: '{statModifierDefinition.Id}') has an empty targetStatId.");
                    continue;
                }

                if (!statExists(modifier.targetStatId))
                {
                    reportError($"[Validation] Asset '{statModifierDefinition.name}' (id: '{statModifierDefinition.Id}') references unknown targetStatId '{modifier.targetStatId}'.");
                }
            }
        }
    }

    public static void ValidateHostStatModifierLinks<THost>(
        IEnumerable<THost> hosts,
        Func<THost, string> hostId,
        Func<THost, IReadOnlyList<string>> hostStatModifierIds,
        Func<THost, string> hostAssetName,
        Func<THost, bool> allowAnyDomain,
        Func<string, bool> statModifierExists,
        Func<string, StatModifierDefinition> getStatModifier,
        Func<string, bool> statExists,
        Func<string, StatDefinition> getStat,
        HashSet<StatDomain> allowedDomains,
        string expectedDomainLabel,
        Action<string> reportError)
    {
        foreach (var host in hosts)
        {
            if (host == null)
                continue;

            var shouldAllowAnyDomain = allowAnyDomain(host);
            var modifierIds = hostStatModifierIds(host);

            foreach (var statModifierId in modifierIds)
            {
                if (string.IsNullOrWhiteSpace(statModifierId))
                {
                    reportError($"[Validation] Asset '{hostAssetName(host)}' (id: '{hostId(host)}') has an empty statModifierId. Expected domain: {expectedDomainLabel}.");
                    continue;
                }

                if (!statModifierExists(statModifierId))
                {
                    reportError($"[Validation] Asset '{hostAssetName(host)}' (id: '{hostId(host)}') references unknown statModifierId '{statModifierId}'. Expected domain: {expectedDomainLabel}.");
                    continue;
                }

                var statModifier = getStatModifier(statModifierId);
                if (statModifier == null)
                    continue;

                foreach (var modifier in statModifier.Modifiers)
                {
                    if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                    {
                        reportError($"[Validation] Asset '{hostAssetName(host)}' (id: '{hostId(host)}') via statModifierId '{statModifierId}' has empty targetStatId. Expected domain: {expectedDomainLabel}.");
                        continue;
                    }

                    if (!statExists(modifier.targetStatId))
                    {
                        reportError($"[Validation] Asset '{hostAssetName(host)}' (id: '{hostId(host)}') via statModifierId '{statModifierId}' references unknown targetStatId '{modifier.targetStatId}'. Expected domain: {expectedDomainLabel}.");
                        continue;
                    }

                    if (shouldAllowAnyDomain)
                        continue;

                    var targetStat = getStat(modifier.targetStatId);
                    if (targetStat == null)
                        continue;

                    if (!allowedDomains.Contains(targetStat.Domain))
                    {
                        reportError($"[Validation] Asset '{hostAssetName(host)}' (id: '{hostId(host)}') via statModifierId '{statModifierId}' targets stat '{modifier.targetStatId}' in domain '{targetStat.Domain}'. Expected domain: {expectedDomainLabel}.");
                    }
                }
            }
        }
    }
}
