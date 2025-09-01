using Business.Abstract;
using Member_System.Languages;
using Member_System.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Member_System.Models.Partner;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Member_System.Enums;
using Data.Response.Company;
using Data.Response;
using Member_System.Models.Company;
using Data.Response.Product;

namespace Member_System.Controllers
{
    public class PartnerController : Controller
    {
        ICompanyService _companyService;
        ISharedService _sharedService;
        IMemberService _memberService;
        IPartnerService _partnerService;
        IProductService _productService;

        private readonly IWebHostEnvironment hostingEnvironment; //file upload için lazım
        HttpClient _companyClient;
        HttpClient _sharedClient;
        HttpClient _productClient;


        private readonly IConfiguration _configuration;

        public PartnerController(ISharedService sharedService, ICompanyService companyService, IHttpClientFactory companyClient, IHttpClientFactory sharedClient,
            IConfiguration configuration, IMemberService memberService, IPartnerService partnerService, IProductService productService, IWebHostEnvironment hostingEnvironment)
        {
            _companyService = companyService;
            _sharedService = sharedService;

            _partnerService = partnerService;
            this.hostingEnvironment = hostingEnvironment;
            _companyClient = companyClient.CreateClient("companyClient");
            _sharedClient = sharedClient.CreateClient("sharedClient");
            _configuration = configuration;
            _memberService = memberService;
            _productService = productService;
        }

        public IActionResult CreatePartner()
        {

            #region viewbag
            var taxOfficeResponse = _sharedService.GetAllTaxOffice();
            var listTaxOffice = taxOfficeResponse.Result.value;
            ViewBag.TaxOfficeList = listTaxOffice;
            ViewData["TaxOfficeList"] = listTaxOffice;

            var sectorResponse = _sharedService.GetAllSector();
            var listSector = sectorResponse.Result.value;
            ViewBag.SectorList = listSector;
            ViewData["SectorList"] = listSector;


            var departmentResponse = _sharedService.GetAllDepartment();
            var listDepartment = departmentResponse.Result.value;
            ViewBag.DepartmentList = listDepartment;


            var titleResponse = _sharedService.GetAllTitle();
            var listTitle = titleResponse.Result.value;
            ViewBag.TitleList = listTitle;



            var countryResponse = _sharedService.GetAllCountry();
            var listCountry = countryResponse.Result.value;
            ViewBag.CountryList = listCountry;



            //var cityResponse = _sharedService.GetAllCity();
            //var listCity = cityResponse.Result.value;
            //ViewBag.CityList = listCity;


            //var provinceResponse = _sharedService.GetAllProvince();
            //var provinceList = provinceResponse.Result.value;
            //ViewBag.ProvinceList = provinceList;


            var postCodeResponse = _sharedService.GetAllPostCode();
            var postCodeList = postCodeResponse.Result.value;
            ViewBag.PostCodeList = postCodeList;
            #endregion
            return View();
        }

       
        public async Task<IActionResult> PartnerList()
        {
            try
            {
                var organizationId = JsonConvert.DeserializeObject<string>(HttpContext.Session.GetString("organizationId"));

                var list = await _partnerService.GetPartnerAccountByOrganizationId(organizationId);

                string userTypeStr = HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                UserType userType;
                if (!Enum.TryParse(userTypeStr, out userType))
                    userType = UserType.Customer;

                ViewBag.UserType = userType;
                return View("PartnerList", list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Sunucu hatası oluştu.");
            }

        }
        [HttpGet]
        public async Task<IActionResult> PartnerDetails(string partnerId)
        {
            try
            {
                string userTypeStr = HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                UserType userType;
                if (!Enum.TryParse(userTypeStr, out userType))
                    userType = UserType.Customer;

                ViewBag.UserType = userType;
                var partner = await _partnerService.GetPartnerAccountById(partnerId);

                var result = await _companyService.GetUserCompanyAccountByPartnerId(partnerId);
                if (!result.succeeded || result.value == null)
                    return View(new List<UserCompanyAccountListViewModel>());

                var list = new List<UserCompanyAccountListViewModel>();

                var productCache = new Dictionary<Guid, GetProductByIdRepository.Value>();
                var versionCache = new Dictionary<Guid, GetProductVersionByIdRepository.Value>();
                var familyCache = new Dictionary<Guid, GetProductFamilByIdRepository.Value>();

                var tasks = result.value.Select(async item =>
                {
                    // Şehir ve ilçe
                    var cityTask = string.IsNullOrEmpty(item.city_ID)
                        ? Task.FromResult<string>("")
                        : _sharedService.GetCityById(item.city_ID).ContinueWith(t => t.Result?.value?.city_Name ?? "");
                    var provinceTask = string.IsNullOrEmpty(item.province_ID)
                        ? Task.FromResult<string>("")
                        : _sharedService.GetProvinceById(item.province_ID).ContinueWith(t => t.Result?.value?.province_Name ?? "");
                    var taxTask = !string.IsNullOrEmpty(item.taxOffice_ID)
                        ? _sharedService.GetTaxOfficeById(item.taxOffice_ID)
                        : null;

                    await Task.WhenAll(cityTask, provinceTask);
                    string cityName = cityTask.Result;
                    string provinceName = provinceTask.Result;
                    string taxName = taxTask?.Result?.value?.tax_Office_Name ?? "";

                    // Registry point ve lisanslar
                    var registryPoints = await _companyService.GetUserCompanyRegistryPointByUserCompanyId(item.userCompany_ID);
                    if (registryPoints?.value == null) return;

                    var licenseTasks = registryPoints.value.Select(rp => _companyService.GetUserCompanyLicenseByRegistryPointId(rp.UserCompany_Registry_Point_ID));
                    var licenseResults = await Task.WhenAll(licenseTasks);

                    var productList = new List<GetProductByIdRepository.Value>();
                    var versionList = new List<GetProductVersionByIdRepository.Value>();
                    var familyList = new List<GetProductFamilByIdRepository.Value>();

                    foreach (var licenses in licenseResults)
                    {
                        if (licenses?.value == null) continue;

                        var innerTasks = licenses.value.Select(async license =>
                        {
                            if (license == null) return;

                            // Product
                            GetProductByIdRepository.Value product = null;
                            if (!productCache.TryGetValue(license.Product_ID, out product))
                            {
                                var productResult = await _productService.GetProductById(license.Product_ID.ToString());
                                if (!productResult.succeeded || productResult.value == null) return;
                                product = productResult.value;
                                productCache[license.Product_ID] = product;
                            }
                            if (product != null) productList.Add(product);

                            // Version
                            GetProductVersionByIdRepository.Value version = null;
                            if (product != null && !versionCache.TryGetValue(product.Product_Version_ID, out version))
                            {
                                var versionResult = await _productService.GetProductVersionById(product.Product_Version_ID.ToString());
                                if (!versionResult.succeeded || versionResult.value == null) return;
                                version = versionResult.value;
                                versionCache[product.Product_Version_ID] = version;
                            }
                            if (version != null) versionList.Add(version);

                            // Family
                            GetProductFamilByIdRepository.Value family = null;
                            if (product != null && !familyCache.TryGetValue(product.Product_Family_ID, out family))
                            {
                                var familyResult = await _productService.GetProductFamilyById(product.Product_Family_ID.ToString());
                                if (!familyResult.succeeded || familyResult.value == null) return;
                                family = familyResult.value;
                                familyCache[product.Product_Family_ID] = family;
                            }
                            if (family != null) familyList.Add(family);
                        });

                        await Task.WhenAll(innerTasks);
                    }

                    // ViewModel oluştur
                    var vm = new UserCompanyAccountListViewModel
                    {
                        UserCompany_Identity_No = item.userCompany_Identity_No,
                        UserCompany_Name = item.userCompany_Name,
                        UserCompany_Surname = item.userCompany_Surname,
                        Phone = $"{item.gsM_Prefix} {item.gsM_Number}",
                        Representative_Name = item.representative_NameAndSurename,
                        Email = item.eMail,
                        Representative_Phone = item.representative_GSM_Number,
                        Status = item.is_Passive ? "Pasif" : "Aktif",
                        CityName = cityName,
                        ProvinceName = provinceName,
                        FamilyList = familyList,
                        ProductList = productList,
                        VersionList = versionList,
                        Partner = partner?.value,
                        TaxName = taxName,
                        UserCompany_ID = Guid.Parse(item.userCompany_ID)
                    };

                    lock (list)
                    {
                        list.Add(vm);
                    }
                });

                await Task.WhenAll(tasks);

                return PartialView(list);
            }
            catch
            {
                return StatusCode(500, "Sunucu hatası oluştu.");
            }        
        }
    }
}
