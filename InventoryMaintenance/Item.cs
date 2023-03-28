using System;

namespace InventoryMaintenance
{
    public class Item
    {
        public String Type { get; set; } = "";  // "Normal New Items", "Update Worksheet", or "New Deli Items"

        public String Filename { get; set; } = "";
        public String Row { get; set; } = "";
        public String Initials { get; set; } = "";
        public String GMO { get; set; } = "";
        public String Department { get; set; } = "";
        public String UPC { get; set; } = "";
        public String Brand { get; set; } = "";
        public String ItemName { get; set; } = "";
        public String Size { get; set; } = "";
        public String ReceiptAlias { get; set; } = "";
        public String Supplier { get; set; } = "";
        public String SupplierID { get; set; } = "";
        public String CaseDescription { get; set; } = "";
        public String UnitsPerCase { get; set; } = "";
        public String Category { get; set; } = "";
        public String Subcategory { get; set; } = "";
        public String SupplierType { get; set; } = "";
        public String DeliScalePrice { get; set; } = "";
        public String DeliWeight { get; set; } = "";
        public String DeliExpiration { get; set; } = "";
        public String Ingredients { get; set; } = "";
        public String ContainerType { get; set; } = "";
        public bool EBT { get; set; } = false;
        public String AltID { get; set; } = "";
        public String AltReceiptAlias { get; set; } = "";
        public String QTYMultiplier { get; set; } = "";
        public String ExceptionPricing { get; set; } = "";
        public String Notes { get; set; } = "";
        public String PWF8 { get; set; } = "";
        
        public bool PrintSupplier = true;

        public decimal manualMargin = 0.0m;
        public String IdealMargin
        {
            get
            {
                decimal value = 0.0m;

                if (manualMargin != 0.0m)
                {
                    value = Math.Round(manualMargin, 2);
                    return value.ToString();
                }
                else
                {
                    value = GetIdealMargin() * 100;
                    value = Math.Round(value, 2);
                    return value.ToString();
                }
            }

            set
            {
                manualMargin = decimal.TryParse(value, out decimal result) ? result : 0.0m;

                if (manualMargin > 99)
                    manualMargin = 99;
                else if (manualMargin < 1)
                    manualMargin = 1;
            }
        }

        decimal cost = 0.0m;
        public String Cost
        {
            get { return Math.Round(cost, 2).ToString(); }
            set => cost = decimal.TryParse(value, out decimal result) ? result : 0.0m;
        }

        public String oldPrice = "";

        public decimal Price
        {
            get
            {
                if (ExceptionPricing != "")
                    return decimal.TryParse(ExceptionPricing, out decimal result2) ? result2 : 0.0m;

                if (DeliScalePrice != "")
                    return decimal.TryParse(DeliScalePrice, out decimal result1) ? result1 : 0.0m;

                if (cost == 0.0m)
                    return 0.0m;

                decimal newPrice = 0.0m;
                decimal total = cost / (1 - (decimal.Parse(IdealMargin)/100));
                total = Math.Round(total, 2);
                int dollars = (int)total;
                int cents = (int)((total - dollars) * 100);

                // Code removed to protect the privacy
                // of company this was written for.
                // cents rounding goes here.

                return newPrice;
            }
        }

        public String ActualMargin
        {
            get
            {
                if (cost == 0.0m || Price == 0.0m)
                    return "0";

                decimal result = Math.Abs((cost / Price) - 1);
                result *= 100;
                result = Math.Round(result, 2);
                return result.ToString();
            }
        }

        public decimal GetActualMargin()
        {
            return decimal.TryParse(ActualMargin, out decimal result) ? result : 0;
        }

        public decimal GetIdealMargin()
        {
            switch (Supplier)
            {
                // Code removed to protect the privacy
                // of company this was written for.
                default:
                    break;
            }

            switch (Department)
            {
                // Code removed to protect the privacy
                // of company this was written for.
                default:
                    break;
            }

            return 0.0m;
        }
    }
}
