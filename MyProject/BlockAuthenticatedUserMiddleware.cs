namespace MyProject
{
    public class BlockAuthenticatedUserMiddleware
    {
        private readonly RequestDelegate next;

        public BlockAuthenticatedUserMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true && 
               (context.Request.Path.StartsWithSegments("/Auth/Register")|| 
                context.Request.Path.StartsWithSegments("/Auth/Login") ||
                context.Request.Path.StartsWithSegments("/Auth/VerifyOtp"))) {

                context.Response.Redirect("/Home/Index");
                return;
            }

            await next(context);
        }
    }
}
