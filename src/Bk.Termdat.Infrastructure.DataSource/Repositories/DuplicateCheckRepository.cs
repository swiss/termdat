using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bk.Termdat.Business.Repositories;
using Bk.Termdat.Business.Resource.Models;
using Microsoft.EntityFrameworkCore;

namespace Bk.Termdat.Infrastructure.DataSource.Repositories;

public class DuplicateCheckRepository : IDuplicateCheckRepository
{
    private readonly DataContext context;

    public DuplicateCheckRepository(DataContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Finds duplicate entries for all entries in the specified collection by comparing their terminology and names
    /// with all entries in the specified language.
    /// </summary>
    /// <returns>
    /// A collection of duplicate matches, where each entry contains the matching duplicate entry IDs.
    /// </returns>
    public async Task<IReadOnlyList<DuplicateCheckMatch>> GetByCollectionIdAndLanguageId(
        int collectionId,
        int languageId,
        CancellationToken cancellationToken = default)
    {
        // Raw SQL is used here because the query relies on multiple CTEs, UNION ALL, and PostgreSQL-specific
        // array_agg aggregation, which would be significantly more complex and less readable in LINQ.

        return await context.Database.SqlQuery<DuplicateCheckMatch>(
            $"""
        WITH collection_keys AS (
            SELECT
                el.entry_id AS collection_entry_id,
                lower(el.terminus) AS match_key,
                el.terminus AS match_text
            FROM data.entry_languages el
            JOIN data.entries e ON e.id = el.entry_id
            WHERE e.collection_id = {collectionId}
              AND el.language_id = {languageId}
              AND el.terminus IS NOT NULL

            UNION ALL

            SELECT
                el.entry_id,
                lower(el.name),
                el.name
            FROM data.entry_languages el
            JOIN data.entries e ON e.id = el.entry_id
            WHERE e.collection_id = {collectionId}
              AND el.language_id = {languageId}
              AND el.name IS NOT NULL
        ),
        all_keys AS (
            SELECT
                el.entry_id,
                lower(el.terminus) AS match_key,
                el.terminus AS match_text
            FROM data.entry_languages el
            WHERE el.language_id = {languageId}
              AND el.terminus IS NOT NULL

            UNION ALL

            SELECT
                el.entry_id,
                lower(el.name),
                el.name
            FROM data.entry_languages el
            WHERE el.language_id = {languageId}
              AND el.name IS NOT NULL
        ),
        matches AS (
            SELECT DISTINCT
                ck.collection_entry_id,
                ak.entry_id AS matched_entry_id,
                ck.match_text
            FROM collection_keys ck
            JOIN all_keys ak
              ON ak.match_key = ck.match_key
             AND ak.entry_id <> ck.collection_entry_id
        ),
        pair_matches AS (
            SELECT
                collection_entry_id,
                matched_entry_id,
                string_agg(DISTINCT match_text, ', ' ORDER BY match_text) AS matched_text
            FROM matches
            GROUP BY collection_entry_id, matched_entry_id
        )
        SELECT
            collection_entry_id AS entry_id,
            array_agg(matched_entry_id ORDER BY matched_entry_id) AS matched_entry_ids,
            array_agg(matched_text ORDER BY matched_entry_id) AS matched_keys
        FROM pair_matches
        GROUP BY collection_entry_id
        ORDER BY collection_entry_id;
        """)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds duplicate entries for all entries in the specified glossary by comparing their terminology and names
    /// with all entries in the specified language.
    /// </summary>
    /// <returns>
    /// A collection of duplicate matches, where each entry contains the matching duplicate entry IDs.
    /// </returns>
    public async Task<IReadOnlyList<DuplicateCheckMatch>> GetByGlossaryIdAndLanguageId(
            int glossaryId,
            int languageId,
            CancellationToken cancellationToken = default)
    {
        // Raw SQL is used here because the query relies on multiple CTEs, UNION ALL, and PostgreSQL-specific
        // array_agg aggregation, which would be significantly more complex and less readable in LINQ.

        return await context.Database.SqlQuery<DuplicateCheckMatch>(
            $"""
        WITH glossary_keys AS (
            SELECT
                el.entry_id AS glossary_entry_id,
                lower(el.terminus) AS match_key,
                el.terminus AS match_text
            FROM data.entry_languages el
            JOIN data.entry_glossary eg ON eg.entries_id = el.entry_id
            WHERE eg.glossaries_id = {glossaryId}
              AND el.language_id = {languageId}
              AND el.terminus IS NOT NULL

            UNION ALL

            SELECT
                el.entry_id,
                lower(el.name),
                el.name
            FROM data.entry_languages el
            JOIN data.entry_glossary eg ON eg.entries_id = el.entry_id
            WHERE eg.glossaries_id = {glossaryId}
              AND el.language_id = {languageId}
              AND el.name IS NOT NULL
        ),
        all_keys AS (
            SELECT
                el.entry_id,
                lower(el.terminus) AS match_key,
                el.terminus AS match_text
            FROM data.entry_languages el
            WHERE el.language_id = {languageId}
              AND el.terminus IS NOT NULL

            UNION ALL

            SELECT
                el.entry_id,
                lower(el.name),
                el.name
            FROM data.entry_languages el
            WHERE el.language_id = {languageId}
              AND el.name IS NOT NULL
        ),
        matches AS (
            SELECT DISTINCT
                ck.glossary_entry_id,
                ak.entry_id AS matched_entry_id,
                ck.match_text
            FROM glossary_keys ck
            JOIN all_keys ak
              ON ak.match_key = ck.match_key
             AND ak.entry_id <> ck.glossary_entry_id
        ),
        pair_matches AS (
            SELECT
                glossary_entry_id,
                matched_entry_id,
                string_agg(DISTINCT match_text, ', ' ORDER BY match_text) AS matched_text
            FROM matches
            GROUP BY glossary_entry_id, matched_entry_id
        )
        SELECT
            glossary_entry_id AS entry_id,
            array_agg(matched_entry_id ORDER BY matched_entry_id) AS matched_entry_ids,
            array_agg(matched_text ORDER BY matched_entry_id) AS matched_keys
        FROM pair_matches
        GROUP BY glossary_entry_id
        ORDER BY glossary_entry_id;
        """)
            .ToListAsync(cancellationToken);
    }
}
