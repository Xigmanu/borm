using Borm.Model.Metadata;

namespace Borm.Model.Validation;

internal delegate ValidationResult ObjectValidator(object entityObj, IEntityMetadata metadata);