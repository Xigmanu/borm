using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Model.Validators;

namespace Borm;

internal sealed class ContextInitializer
{
    private readonly IValidator<IReadOnlyList<EntityInfo>> _modelValidator;
    private bool _isInitialized;

    public ContextInitializer(IValidator<IReadOnlyList<EntityInfo>> modelValidator)
    {
        _modelValidator = modelValidator;
        _isInitialized = false;
    }

    public void Initialize(DataContext context)
    {
        IReadOnlyList<EntityInfo> model = context.Model;
        if (_isInitialized || model.Count == 0)
        {
            return;
        }

        _modelValidator.Validate(model);

        List<IEntityMetadata> metadata = [.. model.Select(EntityMetadataFactory.Create)];

        new TableGraphBuilder(metadata).Build(context.TableGraph);
        context.DataSynchronizer.SyncSchemaWithDataSource();

        _isInitialized = true;
    }
}
