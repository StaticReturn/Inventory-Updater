using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMaintenance
{
    class Width
    {
        public bool initials = false;
        public bool gmo = false;
        public bool department = false;
        public bool upc = false;
        public bool brand = false;
        public bool itemName = false;
        public bool size = false;
        public bool receiptAlias = false;
        public bool supplier = false;
        public bool supplierID = false;
        public bool caseDescription = false;
        public bool unitsPerCase = false;
        public bool category = false;
        public bool subcategory = false;
        public bool supplierType = false;
        public bool cost = false;
        public bool containerType = false;
        public bool ebt = false;
        public bool altID = false;
        public bool altReceiptAlias = false;
        public bool qtyMultiplier = false;
        public bool exceptionPricing = false;
        public bool notes = false;
        public bool pwf8 = false;
        public bool idealMargin = false;
        public bool price = false;
        public bool actualMargin = false;


        public int Count
        {
            get
            {
                int count = 0;
                count += initials ? 1 : 0;
                count += gmo ? 1 : 0;
                count += department ? 1 : 0;
                count += upc ? 1 : 0;
                count += brand ? 1 : 0;
                count += itemName ? 1 : 0;
                count += size ? 1 : 0;
                count += receiptAlias ? 1 : 0;
                count += supplier ? 1 : 0;
                count += supplierID ? 1 : 0;
                count += caseDescription ? 1 : 0;
                count += unitsPerCase ? 1 : 0;
                count += category ? 1 : 0;
                count += subcategory ? 1 : 0;
                count += supplierType ? 1 : 0;
                count += cost ? 1 : 0;
                count += containerType ? 1 : 0;
                count += ebt ? 1 : 0;
                count += altID ? 1 : 0;
                count += altReceiptAlias ? 1 : 0;
                count += qtyMultiplier ? 1 : 0;
                count += exceptionPricing ? 1 : 0;
                count += notes ? 1 : 0;
                count += pwf8 ? 1 : 0;
                count += idealMargin ? 1 : 0;
                count += price ? 1 : 0;
                count += actualMargin ? 1 : 0;

                return count;
            }
        }
    }
}
