using Nice3point.Revit.Toolkit.External.Handlers;

namespace ExporterModels.services;

public static class Handlers
{
    private static ActionEventHandler? _actionEventHandler;
    private static AsyncEventHandler? _asyncEventHandler;
    
    public static ActionEventHandler ActionEventHandler
    {
        get => _actionEventHandler ?? throw new InvalidOperationException("The Handler was never set.");
        private set => _actionEventHandler = value;
    }

    public static AsyncEventHandler AsyncEventHandler
    {
        get => _asyncEventHandler ?? throw new InvalidOperationException("The Handler was never set.");
        private set => _asyncEventHandler = value;
    }

    public static void RegisterHandlers()
    {
        ActionEventHandler = new ActionEventHandler();
        AsyncEventHandler = new AsyncEventHandler();
    }
}