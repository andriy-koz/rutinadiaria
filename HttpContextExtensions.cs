using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

public static class HttpContextExtensions
{
    public static async Task RenderViewAsync<TModel>(this HttpContext context, string viewName, TModel model)
    {
        var services = context.RequestServices;
        var viewEngine = services.GetRequiredService<ICompositeViewEngine>();
        var tempDataProvider = services.GetRequiredService<ITempDataProvider>();

        var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());

        using var writer = new StringWriter();

        var viewResult = viewEngine.FindView(actionContext, viewName, isMainPage: false);

        if (!viewResult.Success)
        {
            throw new FileNotFoundException($"View '{viewName}' not found.");
        }

        var viewDictionary = new ViewDataDictionary<TModel>(
            metadataProvider: new EmptyModelMetadataProvider(),
            modelState: new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);

        var result = writer.ToString();

        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(result);
    }
}
