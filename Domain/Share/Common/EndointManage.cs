namespace Domain.Share.Common
{
    public static class EndpointManage
    {
        public const string ApiVersion = "api/v1";

        public static class ContractReceipts
        {
            public const string CreateReceipt = "create";
            public const string EditReceiptInfo = "edit-receipt" + "/{receiptId}";
            public const string DeleteReceiptInfo = "delete-receipt";
            public const string GetReceipts = "get-receipts";
        }
        public static class ContractSuppliers
        {
            public const string CreateNewContract = "create";
            public const string AddProductToSupContract = "add-product-to-contract" + "/{supplierContractId}";
            public const string DeleteProductFromSupContract = "delete-product-from-contract";
            public const string GetSupplierContracts = "supplier-contracts";
            public const string GetSupplierByContract = "get-supplier-by-contract" + "/{supplierContractId}";
            public const string GetDetail = "get-detail" + "/{supplierContractId}";
            public const string UpdateQuantityProductFromSupContract = "update-quantity-product-from-contract";
            public const string EditContractSup = "edit-contract-sup" + "/{supplierContractId}";

        }
        public static class Contract
        {
            public const string CreateNewContract = "create";
            public const string AddProductToContract = "add-product-to-contract" + "/{contractId}";
            public const string DeleteProductFromContract = "delete-product-from-contract";
            public const string UpdateProductQuantityFromContract = "update-product-quantity-from-contract";
            public const string UpdateContractInfo = "update-contract-info" + "/{contractId}";
            public const string EditExportedProductFromContract = "edit-exported-product";
            public const string GetContracts = "contracts";
            public const string GetCustomerByContract = "get-customer-by-contract" + "/{contractId}";
            public const string GetContractDetail = "get-contract-detail";
        }
        public static class Transfer
        {
            private const string Entity = "transfer";
            public const string InternalTransfer = "internal-transfer";
        }
        public static class Quotation
        {
            private const string Entity = "quotations";
            public const string GetAll = Entity;               // GET /quotations
            public const string GetDetail = "get-detail" + "/{quotationId}";    // GET /quotations/{id}
            public const string Create = "create";               // POST /quotations
            public const string Update = "update" + "/{quotationId}";     // PUT /quotations/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /quotations/{id}
            public const string Restore = "restore" + "/{id}";   // PUT /quotations/{id}
            public const string AddProduct = "add-product" + "/{quotationId}"; // POST /quotations/{id}/add-product
            public const string DeleteProduct = "delete-quotation-product"; // DELETE /quotations/{id}/delete-product
            public const string EditProductQuantity = "edit-quantity-product"; // PATCH /quotations/{id}/edit-quantity-product
            public const string GetCustomer = "get-customer-by-quotation/{quotationId}"; //Get/ quotations/get-customer-by-qoutation
            public const string CreateNewCustomer = "create-new-customer/{quotationId}"; //Post /quotations/create-new-customer/{quotationId}
            public const string UploadWithExcel = "upload-excel";
        }
        public static class Department
        {
            private const string Entity = "departments";
            public const string GetAll = Entity;               // GET /departments
            public const string GetDetail = "get-detail" + "/{id}";    // GET /departments/{id}
            public const string Create = "create";               // POST /departments
            public const string Update = "update" + "/{id}";     // PUT /departments/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /departments/{id}
        }

        public static class BankAccounts
        {
            private const string Entity = "bank-accounts";
            public const string GetAll = Entity;               // GET /bank-accounts
            public const string GetDetail = "get-detail" + "/{id}";    // GET /bank-accounts/{id}
            public const string Create = "create";               // POST /bank-accounts
            public const string Update = "update" + "/{id}";     // PUT /bank-accounts/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /bank-accounts/{id}
            public const string Restore = "restore" + "/{id}";   // PUT /bank-accounts/{id}
        }
        public static class Employee
        {
            private const string Entity = "employees";

            public const string GetAll = Entity;               // GET /employees
            public const string GetDetail = "get-detail" + "/{id}";    // GET /employees/{id}
            public const string Create = "create";               // POST /employees
            public const string Update = "update" + "/{id}";     // PUT /employees/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /employees/{id}
            public const string Restore = "restore" + "/{id}";   // PUT /employees/{id}
        };
        public static class EmployeeType
        {
            private const string Entity = "employee-types";
            public const string GetAll = Entity;               // GET /employee-types
            public const string GetDetail = "get-detail" + "/{id}";    // GET /employee-types/{id}
            public const string Create = "create";               // POST /employee-types
            public const string Update = "update" + "/{id}";     // PUT /employee-types/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /employee-types/{id}
            public const string Resotre = "resotre" + "/{id}";     // DELETE /employee-types/{id}
        }
        public static class Suppliers
        {
            private const string Entity = "suppliers";
            public const string GetAll = Entity;               // GET /suppliers
            public const string GetDetail = "get-detail" + "/{id}";    // GET /suppliers/{id}
            public const string Create = "create";               // POST /suppliers
            public const string Update = "update" + "/{id}";     // PUT /suppliers/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /suppliers/{id}
            public const string Restore = "restore" + "/{id}";
        }
        public static class Unit
        {
            private const string Entity = "units";
            public const string GetAll = Entity;               // GET /units
            public const string GetDetail = "get-detail" + "/{id}";    // GET /units/{id}
            public const string Create = "create";               // POST /units
            public const string Update = "update" + "/{id}";     // PUT /units/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /units/{id}

        }
        public static class Customer
        {
            private const string Entity = "customers";
            public const string GetAll = Entity;               // GET /customers
            public const string GetDetail = "get-detail" + "/{id}";    // GET /customers/{id}
            public const string Create = "create";               // POST /customers
            public const string Update = "update" + "/{id}";     // PUT /customers/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /customers/{id}
            public const string Restore = "restore" + "/{id}";     // PUT /units/{id}

        }



        public static class Product
        {
            private const string Entity = "products";
            public const string GetAll = Entity;               // GET /products
            public const string GetDetail = "get-detail" + "/{id}";    // GET /products/{id}
            public const string Create = "create";               // POST /products
            public const string Update = "update" + "/{id}";     // PUT /products/{id}
            public const string Delete = "delete" + "/{id}";     // DELETE /products/{id}
        }
        public static class Category
        {
            private const string Entity = "categories";

            public const string GetAll = Entity;               // GET /categories
            public const string GetById = Entity + "/{id}";    // GET /categories/{id}
            public const string Create = Entity;               // POST /categories
            public const string Update = Entity + "/{id}";     // PUT /categories/{id}
            public const string Delete = Entity + "/{id}";     // DELETE /categories/{id}
        }

        public static class Authenication
        {
            private const string Entity = "auth";
            public const string Login = Entity + "/login";      // POST /auth/login
            public const string Register = Entity + "/register"; // POST /auth/register
            public const string RefreshToken = Entity + "/refresh-token"; // POST /auth/refresh-token
            public const string ChangePassword = Entity + "/change-password"; // PATCH /auth/change-password
        }
    }
}
