using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Utility
{
    // Extension method for Session
    // Has to be static!!
    public static class SessionExtension
    {
        // ASP.NET Core has a default implementation of Session as int - SetInt32
        // if we only need int value from session (number of items in Count)
        // If we need whole object (like ShoppingCart) we can use this  Get/Set implementation
        // Register in StartUp.cs
        // This is just an example, it's not used in this tutorial. Default impl is used(SetInt32)
        // Implementation in HomeController Details-Post

        // Implement Session
        // In Session we will store number of items in the shopping cart
        // var count = _unitOfWork.ShoppingCart
            //.GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
            //.ToList().Count();

        // add to session
        // If we want to store an object(List,IEnumerable...) in session use ext method SetObject
        //HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject);

        // get Session object
        //var obj = HttpContext.Session.GetObject<ShoppingCart>(SD.ssShoppingCart);

        // Default built-in session implementation is IntSet32 to get only int type
        public static void SetObject(this ISession session, string key, object value)
        {
            // set session
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // retrieve session object
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
