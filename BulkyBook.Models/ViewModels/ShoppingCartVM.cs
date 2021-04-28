using System;
using System.Collections.Generic;
using System.Text;

namespace BulkyBook.Models.ViewModels
{
    public class ShoppingCartVM
    {
        // We need to display a list of all the items in our shopping cart
        // and we need the User object.
        // If the user belongs to Verified Company and if they are placing an order
        // the email needs to be verified. If it's not ask to send confirmation email.
        // We need something with ApplicationUser and we have it in OrderHeader
        public IEnumerable<ShoppingCart> ListCart { get; set; }
        public OrderHeader OrderHeader { get; set; }

    }
}
