using Microsoft.AspNetCore.Routing.Patterns;

namespace NDiff.Models
{
    public class CustomRoute
    {
        public CustomRoute()
        {
        }

        /// <summary>
        /// Creates a custom route.
        /// </summary>
        /// <param name="route">The route that will be parsed.</param>
        public CustomRoute(string route)
        {
            IsTemplateNull = route is null;
            RoutePattern = RoutePatternFactory.Parse(route ?? "");
        }

        /// <summary>
        /// Contains the base route pattern of controller/method.
        /// </summary>
        public RoutePattern RoutePattern { get; set; }

        /// <summary>
        /// Contains the route Name of the controller/method. Same name must not have different templates. 
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Contains the Order of the Route. 
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Returns if the current <see cref="CustomRoute"/> has a template that is NULL.
        /// </summary>
        private bool IsTemplateNull { get; set; }

        /// <summary>
        /// Returns if the current <see cref="CustomRoute"/> has a Name that is NULL or empty.
        /// </summary>
        public bool IsNameNullOrEmpty => string.IsNullOrEmpty(OperationId);

        /// <summary>
        /// Returns if the current <see cref="CustomRoute"/> has been initialized; has one of the parameters (Template, Name, Order) set.
        /// If the brackets of the constructor of that Route are there but no parameters have been put then it is considered NOT initialized.
        /// </summary>
        public bool IsInitialized => !IsTemplateNull || OperationId != null || Order != null;
    }
}