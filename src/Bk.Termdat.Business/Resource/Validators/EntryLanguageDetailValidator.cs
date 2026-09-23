using Bk.Termdat.Business.Resource.Models;
using Bk.Termdat.Common.Resources;
using FluentValidation;

namespace Bk.Termdat.Business.Resource.Validators;

public class EntryLanguageDetailValidator : AbstractValidator<IEntryLanguageDetail>
{
    public EntryLanguageDetailValidator()
    {
        RuleFor(ld => ld)
            .Must(ld => !string.IsNullOrEmpty(ld.Terminus) || !string.IsNullOrEmpty(ld.Name) ||
                        !string.IsNullOrEmpty(ld.Abbreviation) || !string.IsNullOrEmpty(ld.Phraseology))
            .WithMessage(_ => BackendTexts.LanguageDetailMinimalFields);

        RuleFor(ld => ld.LanguageId)
            .NotEmpty();

        RuleFor(ld => ld.Terminus)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.TerminusSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(_ => BackendTexts.LanguageDetailTerminusRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageTerminus);

        RuleFor(ld => ld.TerminusSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageTerminusSource);

        RuleFor(ld => ld.Name)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.NameSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailNameRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageName);

        RuleFor(ld => ld.NameSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageNameSource);

        RuleFor(ld => ld.Abbreviation)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.AbbreviationSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailAbbreviationRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageAbbreviation);

        RuleFor(ld => ld.AbbreviationSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageAbbreviationSource);

        RuleFor(ld => ld.Phraseology)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.PhraseologySource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailPhraseologyRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguagePhraseology);

        RuleFor(ld => ld.PhraseologySource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguagePhraseologySource);

        When(ld => ld.Sequence == 1, () =>
        {
            RuleFor(ld => ld.Definition)
                .MaximumLength(4000)
                .NotEmpty()
                .When(ld => !string.IsNullOrEmpty(ld.DefinitionSource), ApplyConditionTo.CurrentValidator)
                .WithMessage(BackendTexts.LanguageDetailDefinitionRequiredWhenSourceIsSet)
                .WithName(_ => BackendTexts.EntryLanguageDefinition);
            RuleFor(ld => ld.DefinitionSource)
                .MaximumLength(4000)
                .WithName(_ => BackendTexts.EntryLanguageDefinitionSource);
        }).Otherwise(() =>
        {
            RuleFor(ld => ld.Definition).Empty();
            RuleFor(ld => ld.DefinitionSource).Empty();
        });

        RuleFor(ld => ld.Note)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.NoteSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailNoteRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageNote);

        RuleFor(ld => ld.NoteSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageNoteSource);

        RuleFor(ld => ld.Context)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.ContextSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailContextRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageContext);

        RuleFor(ld => ld.ContextSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageContextSource);

        RuleFor(ld => ld.Country)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageCountry);

        RuleFor(ld => ld.Metadata)
            .MaximumLength(4000)
            .NotEmpty()
            .When(ld => !string.IsNullOrEmpty(ld.MetadataSource), ApplyConditionTo.CurrentValidator)
            .WithMessage(BackendTexts.LanguageDetailMetadataRequiredWhenSourceIsSet)
            .WithName(_ => BackendTexts.EntryLanguageMetadata);

        RuleFor(ld => ld.MetadataSource)
            .MaximumLength(4000)
            .WithName(_ => BackendTexts.EntryLanguageMetadataSource);
    }
}
