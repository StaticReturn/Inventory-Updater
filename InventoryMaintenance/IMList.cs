using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryMaintenance
{
    public class IMList
    {
        public List<Item> items = new List<Item>();
        public int Count { get; private set; } = 0;
        public int NewItemCount { get; private set; } = 0;
        public int NewDeliItemCout { get; private set; } = 0;
        public int UpdateCount { get; private set; } = 0;
        public bool Errors { get; private set; } = false;
        public int EBT { get; private set; } = 0;
        public int Hi5 { get; private set; } = 0;
        public bool DeliLabels { get; private set; } = false;
        public bool EDLP { get; private set; } = false;
        int FiveDigitUPCcount { get; set; } = 0;

        public String InventoryMaintenence(String type)
        {
            String data = "";
            String delimiter = "\t";
            String PWF8letter = "";

            foreach (var item in items)
            {
                if (type == "New Item" && item.Type == "Update Worksheet")
                    continue;

                if (type == "Update" && item.Type != "Update Worksheet")
                    continue;

                PWF8letter = (item.Type == "Update Worksheet") ? "U" : "A";

                data += item.Initials + delimiter; // (A) Initials
                data += "" + delimiter; // (B) Database
                data += (item.GMO == "YES" ? "YES" : "") + delimiter; // (C) Non-GMO
                data += item.Department + delimiter; // (D) Department
                data += item.UPC + delimiter; // (E) UPC
                data += item.Brand + delimiter; // (F) Brand
                data += item.ItemName + delimiter; // (G) Item Name
                data += item.Size + delimiter; // (H) Size
                data += item.ReceiptAlias + delimiter; // (I) Receipt Alias
                data += (item.PrintSupplier ? item.Supplier : "") + delimiter; // (J) Supplier
                data += item.SupplierID + delimiter; // (K) Supplier ID
                data += item.CaseDescription + delimiter; // (L) Case Description
                data += item.UnitsPerCase + delimiter; // (M) Units/Case
                data += item.Category + delimiter; // (N) Category
                data += item.Subcategory + delimiter; // (O) Sub-Category
                data += item.SupplierType + delimiter; // (P) Supplier Type
                data += (item.Price == 0 ? "" : item.Cost) + delimiter; // (Q) Cost
                data += item.DeliScalePrice + delimiter; // (R) Deli Scale Price
                data += item.DeliWeight + delimiter; // (S) Deli Weight
                data += item.ExceptionPricing + delimiter; // (T) Deli Expiration
                data += item.Ingredients + delimiter; // (U) Ingredients
                data += item.ContainerType + delimiter; // (V) Container Type
                data += (item.EBT ? "EBT Food" : "") + delimiter; // (W) EBT
                data += item.AltID + delimiter; // (X) Alt ID
                data += item.AltReceiptAlias + delimiter; // (Y) Alt Receipt Alias
                data += item.QTYMultiplier + delimiter; // (Z) QTY Multiplier
                data += item.ExceptionPricing + delimiter; // (AA) Exceptions
                data += item.Notes + delimiter; // (AB) NOTES
                data += PWF8letter + DateTime.Now.ToString("yy") + "." + DateTime.Now.Month.ToString("00") + (item.EBT ? "e" : "") + (item.ContainerType.Length > 0 ? "h" : "") + delimiter; // (AC) PWF8
                data += item.IdealMargin + delimiter; // (AD) Ideal Margin
                data += (item.Price == 0 ? "" : item.Price.ToString())  + delimiter; // (AE) Price
                data += (item.Price == 0 ? "" : item.ActualMargin) + "\r\n"; // (AF) Actual Margin
            }
            return data;
        }

        public Dictionary<String,String> GetIngredients()
        {
            Dictionary<String, String> ingredients = new Dictionary<string, string>();

            foreach (var item in items)
            {
                if (item.Type != "New Deli Items")
                    continue;

                if (item.Ingredients == "")
                    continue;

                ingredients.Add(item.UPC, item.Ingredients);
            }

            return ingredients;
        }

        public void Add(String filename, int row, String type, List<String> data)
        {
            Item item = new Item();

            item.Type = type;
            item.Filename = filename;
            item.Row = row.ToString();
            item.Initials = data[0];
            item.GMO = data[2];
            item.Department = data[3];
            item.UPC = data[4];
            item.Brand = data[5];
            item.ItemName = data[6];
            item.Size = data[7];
            item.ReceiptAlias = data[8];
            item.Supplier = data[10];
            item.SupplierID = data[11];
            item.CaseDescription = data[12];
            item.UnitsPerCase = data[13];
            item.Category = data[14];
            item.Subcategory = data[15];
            item.SupplierType = data[16];
            item.Cost = data[17];
            item.DeliScalePrice = data[18];
            item.DeliWeight = data[19];
            item.DeliExpiration = data[20];
            item.Ingredients = data[21];
            item.ContainerType = data[22];
            item.EBT = data[23] == "EBT Food" ? true : false;
            item.AltID = data[24];
            item.AltReceiptAlias = data[25];
            item.QTYMultiplier = data[26];
            item.ExceptionPricing = data[27];
            item.Notes = data[28];
            item.PWF8 = data[29];
            item.oldPrice = data[31];

            switch (item.Type)
            {
                case "Normal New Items":
                    NewItemCount++;
                    break;
                case "Update Worksheet":
                    UpdateCount++;
                    break;
                case "New Deli Items":
                    NewDeliItemCout++;
                    break;
                default:
                    break;
            }

            Count++;
            items.Add(item);
        }

        public List<String> GetErrors()
        {
            List<String> errors = new List<String>();
            List<String> supplierIDs = new List<string>();
            List<String> upcs = new List<string>();

            Errors = false;

            foreach (var item in items)
            {
                item.PrintSupplier = true;

                if (item.Department == "")
                    errors.Add($"{item.Filename} ({item.Row}):: No department.");

                if (item.UPC == "")
                    errors.Add($"{item.Filename} ({item.Row}):: No UPC.");

                if (upcs.Contains(item.UPC))
                    errors.Add($"{item.Filename} ({item.Row}):: Duplicate UPC.");

                if (item.Supplier != "" || item.SupplierID != "" || item.CaseDescription != "" || item.UnitsPerCase != "")
                    if (item.Supplier == "" || item.SupplierID == "" || item.CaseDescription == "" || item.UnitsPerCase == "")
                    {
                        if (item.Type == "Normal New Items" || item.Type == "New Deli Items")
                        {
                            errors.Add($"{item.Filename} ({item.Row}):: Not all supplier info given.");
                        } else if (item.Type == "Update Worksheet")
                        {
                            if (item.Supplier != "" && item.SupplierID == "" && item.CaseDescription == "" && item.UnitsPerCase == "")
                                item.PrintSupplier = false;
                            else
                                errors.Add($"{item.Filename} ({item.Row}):: Not all supplier info given.");
                        } else
                        {
                            errors.Add($"{item.Filename} ({item.Row}):: Failed to check supplier info:  Unknown item type: '{item.Type}'");
                        }
                    }

                if (item.AltID != "" || item.AltReceiptAlias != "" || item.QTYMultiplier != "")
                    if (item.AltID == "" || item.AltReceiptAlias == "" || item.QTYMultiplier == "")
                        errors.Add($"{item.Filename} ({item.Row}):: Not all altID info given.");

                if (item.Type == "Normal New Items")
                {
                    if (item.Brand == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No brand.");

                    if (item.ItemName == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No item name.");

                    if (item.Size == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No size.");

                    if (item.ReceiptAlias == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No receipt alias.");

                    if (supplierIDs.Contains(item.SupplierID))
                        errors.Add($"{item.Filename} ({item.Row}):: Dupplicate supplier ID.");

                    if (item.SupplierType != "Local" && item.SupplierType != "Wholesale" && item.SupplierType != "Direct" && item.SupplierType != "")
                        errors.Add($"{item.Filename} ({item.Row}):: Invalid supplier type: '{item.SupplierType}'.");

                    if (item.SupplierType == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No supplier type.");

                    if (item.Cost == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No cost given.");
                } else if (item.Type == "Update Worksheet")
                {

                } else if (item.Type == "New Deli Items")
                {
                    if (item.Brand == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No brand.");

                    if (item.ItemName == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No item name.");

                    if (item.Size == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No size.");

                    if (item.ReceiptAlias == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No receipt alias.");

                    if (item.DeliScalePrice == "")
                        errors.Add($"{item.Filename} ({item.Row}):: No deli scale price given.");
                }
                else
                {
                    errors.Add($"{item.Filename} ({item.Row}):: Unknown item type.");
                }

                if (item.UPC != "")
                    upcs.Add(item.UPC);

                if (item.Price < 0.01m && item.Type != "Update Worksheet")
                    errors.Add($"{item.Filename} ({item.Row}):: Price too low.");

                if (item.Cost != "0.0" && item.Price == 0 && item.Type == "Update Worksheet")
                    errors.Add($"{item.Filename} ({item.Row}):: Price zero.");

                if (item.GetActualMargin() < 25 && item.ExceptionPricing == "" && item.Type != "Update Worksheet")
                    errors.Add($"{item.Filename} ({item.Row}):: ActualMargin of {item.GetActualMargin()} is too low.");
            }

            if (errors.Count > 0)
                Errors = true;

            return errors;
        }

        public List<String> GetWarnings()
        {
            List<String> warnings = new List<String>();

            foreach (var item in items)
            {
                if (item.UPC.Length != 12 && item.UPC.Length != 0 && item.UPC.Length != 5)
                    warnings.Add($"{item.Filename} ({item.Row}):: Unusual UPC length.");

                if (item.Price != (decimal.TryParse(item.oldPrice, out decimal result) ? result : 0.0m) && item.ExceptionPricing == "" && item.manualMargin == 0)
                {
                    if (item.Type != "Update Worksheet" || (item.Type == "Update Worksheet" && item.Price > 0))
                        warnings.Add($"{item.Filename} ({item.Row}):: Worksheet price ({item.oldPrice}) is not calculated price ({item.Price}).");
                }

                String name = item.ItemName.ToUpper();
                String alias = item.ReceiptAlias.ToUpper();

                if (name.Contains("CBD") || alias.Contains("CBD"))
                    warnings.Add($"{item.Filename} ({item.Row}):: CBD item: {item.ItemName}.");

                if (item.Price < 1 && item.Type != "Update Worksheet")
                    warnings.Add($"{item.Filename} ({item.Row}):: Price low.");

                if (item.GetActualMargin() < 30 || (item.GetActualMargin() - item.GetIdealMargin()) < -3)
                {
                    if (item.Type != "Update Worksheet" || (item.Type == "Update Worksheet" && item.Price > 0))
                        warnings.Add($"{item.Filename} ({item.Row}):: ActualMargin of {item.GetActualMargin()} is low.");
                }
            }

            return warnings;
        }

        public void CheckDeliLabels()
        {
            FiveDigitUPCcount = 0;
            DeliLabels = false;

            foreach (var item in items)
            {
                if (item.UPC.Length == 5)
                    FiveDigitUPCcount++;

                if (item.Type == "New Deli Items")
                    DeliLabels = true;
            }

            if (FiveDigitUPCcount > 0)
                DeliLabels = true;
        }

        public void CheckEDLP()
        {
            EDLP = false;

            foreach (var item in items)
            {
                switch (item.Brand)
                {
                    // Code removed to protect the privacy
                    // of company this was written for.
                    default:
                        break;
                }
            }
        }

        public void CheckHi5AndEBT()
        {
            EBT = 0;
            Hi5 = 0;

            foreach (var item in items)
            {
                if (item.EBT)
                    EBT++;

                if (item.ContainerType != "")
                    Hi5++;
            }
        }
    }
}

