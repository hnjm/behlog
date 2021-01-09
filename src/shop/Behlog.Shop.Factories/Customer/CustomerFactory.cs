﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Behlog.Core;
using Behlog.Core.Extensions;
using Behlog.Core.Models.Enum;
using Behlog.Core.Models.Shop;
using Behlog.Shop.Services.Data;
using Behlog.Shop.Factories.Extensions;
using Behlog.Shop.Factories.Contracts;
using Behlog.Core.Contracts.Services.Common;

namespace Behlog.Shop.Factories
{
    public class CustomerFactory: ICustomerFactory {

        private readonly IDateService _dateService;
        private readonly IWebsiteInfo _websiteInfo;
        private readonly IShippingAddressFactory _shippingAddressFactory;

        public CustomerFactory(
            IDateService dateService,
            IWebsiteInfo websiteInfo,
            IShippingAddressFactory shippingAddressFactory) {

            dateService.CheckArgumentIsNull(nameof(dateService));
            _dateService = dateService;

            websiteInfo.CheckArgumentIsNull(nameof(websiteInfo));
            _websiteInfo = websiteInfo;

            shippingAddressFactory.CheckArgumentIsNull(nameof(shippingAddressFactory));
            _shippingAddressFactory = shippingAddressFactory;
        }

        public Customer BuildRealCustomerFromOrder(OrderSingleProductDto model) {
            model.CheckArgumentIsNull(nameof(model));
            var customer = new Customer {
                CreateDate = _dateService.UtcNow(),
                ModifyDate = _dateService.UtcNow(),
                Email = model.Email.Trim(),
                FirstName = model.FirstName.ApplyCorrectYeKe().Trim(),
                LastName = model.LastName.ApplyCorrectYeKe().Trim(),
                Mobile = model.Mobile.Trim(),
                PersonalityType = CustomerPersonalityType.Real,
                NationalCode = model.NationalCode.Trim(),
                Status = CustomerStatus.Enabled,
                WebsiteId = _websiteInfo.Id
            };
            customer.ShippingAddresses
                .Add(_shippingAddressFactory
                        .BuildShippingAddress(model.ShippingAddress));
            return customer;
        }


        public async Task<Invoice> AddInvoiceAsync(Customer customer, 
            IEnumerable<Order> orders,
            ShippingAddress address,
            Shipping shipping) {
            
            var invoice = new Invoice {
                CreateDate = _dateService.UtcNow(),
                ModifyDate = _dateService.UtcNow(),
                ShippingAddress = address,
                Shipping = shipping,
                Status = InvoiceStatus.Issued,
                TotalPrice = orders.Calculate(),
                WebsiteId = _websiteInfo.Id
            };

            customer.Invoices.Add(invoice);

            return await Task.FromResult(invoice);
        }

    }
}
