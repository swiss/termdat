using System;
using System.Collections.Generic;
using System.Linq;
using Bk.Termdat.Business.Domain.Models;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

internal static class UpdateTranslationHelper
{
    internal static void UpdateTranslations<T>(ICollection<T> existing, ICollection<T> update, int existingId, string entityName)
        where T : IEntityNameTranslation
    {
        foreach (var updateTranslation in update)
        {
            var existingTranslations =
                existing.Where(t => t.LanguageId == updateTranslation.LanguageId).ToList();
            if (existingTranslations.Any())
            {
                if (existingTranslations.Count != 1)
                {
                    throw new InvalidOperationException(
                        $"{entityName} with id='{existingId}' has multiple translations for language id='{updateTranslation.LanguageId}'.");
                }

                var existingTranslation = existingTranslations.Single();
                existingTranslation.Name = updateTranslation.Name;
            }
            else
            {
                existing.Add(updateTranslation);
            }
        }
    }

}
