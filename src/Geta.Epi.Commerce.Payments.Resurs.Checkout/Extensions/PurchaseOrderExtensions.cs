using System;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace Geta.Epi.Commerce.Payments.Resurs.Checkout.Extensions
{
    public static class PurchaseOrderExtensions
    {
        public static void AddNote(this PurchaseOrder order, string noteTitle, string noteMessage)
        {
            var note = order.OrderNotes.AddNew();
            note.CustomerId = CustomerContext.Current.CurrentContactId;
            note.Type = OrderNoteTypes.Custom.ToString();
            note.Title = noteTitle;
            note.Detail = noteMessage;
            note.Created = DateTime.UtcNow;

            order.AcceptChanges();
        }
    }
}