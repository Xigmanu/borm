using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Conversion.Internal;
using Borm.Model.Metadata;
using Borm.Model.Metadata.Internal;
using Borm.Model.Validation;

namespace Borm;

internal sealed class ContextInitializer
{
    private readonly MetadataFactory<EntityInfo, IEntityMetadata> _entityMetadataFactory =
        new EntityMetadataFactory(new EntityBufferConversionFactory(), new ColumnMetadataFactory());

    private readonly ModelRelationsValidator _modelValidator = new();
    private bool _isInitialized;

    public void Initialize(DataContext context)
    {
        IReadOnlyList<EntityInfo> model = context.Model;
        if (_isInitialized || model.Count == 0)
        {
            return;
        }

        _modelValidator.Validate(model);

        List<IEntityMetadata> metadata =
        [
            .. model.Select(typeInfo => _entityMetadataFactory.Create(typeInfo))
        ];

        new TableGraphBuilder(metadata).Build(context.TableGraph);
        context.DataSynchronizer.SyncSchemaWithDataSource();

        _isInitialized = true;
    }
}