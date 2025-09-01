
using Business.Abstract;
using Data.Response;
using Data.Response.Company;
using Data.Response.Partner;
using Data.Response.Product;
using Member_System.Enums;
using Member_System.Models;
using Member_System.Models.Company;
using Member_System.Models.Member_Authorized;
using Member_System.Models.Partner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Member_System.Controllers
{
    public class CompanyController : Controller
    {
        ICompanyService _companyService;
        ISharedService _sharedService;
        IMemberService _memberService;
        IPartnerService _partnerService;
        IProductService _productService;
        ILicenseService _licenseService;

        private readonly IWebHostEnvironment hostingEnvironment; //file upload için lazım
        HttpClient _companyClient;
        HttpClient _sharedClient;
        private Partial partial = new Partial();


        private readonly IConfiguration _configuration;

        public CompanyController(ISharedService sharedService, ICompanyService companyService, IHttpClientFactory companyClient, IHttpClientFactory sharedClient,
            IConfiguration configuration, IMemberService memberService, IPartnerService partnerService, IProductService productService, ILicenseService licenseService, IWebHostEnvironment hostingEnvironment)
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
            _licenseService = licenseService;
        }

        public async Task<PartialViewResult> SearchPartnerChangeRequest(string searchText)
        {


            var response = await _partnerService.GetAllRequestChangePartner(this._configuration.GetSection("Information")["Organization_ID"], this._configuration.GetSection("Information")["Organization_ID"]);
            var responseContent = response.value;
            List<PartnerListViewModel> partnerChangeRequestLists = new List<PartnerListViewModel>();
            List<string> status = new List<string>();
            if (response.succeeded)
            {
                foreach (var item in response.value)
                {
                    var partnerAccountResponse = await _partnerService.GetPartnerAccountById(item.partner_ID);
                    if (partnerAccountResponse.value == null)
                    {




                    }
                    else
                    {
                        var cityResponse = await _sharedService.GetCityById(partnerAccountResponse.value.city_ID);
                        var titleResponse = await _sharedService.GetTitleById(partnerAccountResponse.value.title_ID);
                        var provinceResponse = await _sharedService.GetProvinceById(partnerAccountResponse.value.province_ID);

                        if (cityResponse.succeeded)
                        {

                            partnerAccountResponse.value.city_ID = cityResponse.value.city_Name;
                        }
                        if (titleResponse.succeeded)
                        {


                            partnerAccountResponse.value.title_ID = titleResponse.value.title_Name;
                        }
                        if (provinceResponse.succeeded)
                        {


                            partnerAccountResponse.value.province_ID = provinceResponse.value.province_Name;
                        }
                        if (item.status == 0)
                        {

                            status.Add("Beklemede");

                        }
                        else if (item.status == 1)
                        {
                            status.Add("Onaylandı");

                        }
                        else
                        {
                            status.Add("Reddedildi");

                        }
                        var model = new PartnerListViewModel
                        {

                            PartnerList = partnerAccountResponse.value,
                            Request_ID = item.request_ID,

                        };
                        partnerChangeRequestLists.Add(model);

                        ViewBag.Status = status;
                    }

                }
                ViewBag.RequestList = partnerChangeRequestLists;

                var result = partnerChangeRequestLists;

                ViewBag.RequestList = result;
            }
            return PartialView("_PartnerChangeRequestListTable", ViewBag.RequestList);

        }

        [HttpPost]
        public async Task<IActionResult> ApproveSubCompany(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { succeeded = false, message = "Geçersiz şirket ID'si." });
                }

                Guid userCompanyIdGuid;
                if (!Guid.TryParse(id, out userCompanyIdGuid))
                {
                    return Json(new { succeeded = false, message = "Geçersiz ID formatı." });
                }

                // kaydı güncelle
                //userCompanyAcc.value.Is_SubUserCompany = true;
                var updateModel = new
                {
                    UserCompany_ID = userCompanyIdGuid,
                    Is_SubUserCompany = true
                };
                var updateResult = await _companyService.UpdateUserCompanyAccountById(updateModel);


                return Json(new { succeeded = true, message = "Şirket başarıyla onaylandı." });
            }
            catch (Exception ex)
            {
                // Hata durumunda, hata mesajını döndür
                return Json(new { succeeded = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

   
            GetUserCompanyAccountByMemberUserIdResponse connectedCompany = new GetUserCompanyAccountByMemberUserIdResponse();
            GetUserCompanyAccountByFilterResponse subCompanys = new GetUserCompanyAccountByFilterResponse();

            if (!string.IsNullOrWhiteSpace(company.Connected_UserCompany_ID) && Guid.TryParse(company.Connected_UserCompany_ID, out Guid connectedId))
            {
                if (connectedId != Guid.Empty)
                {
                    connectedCompany = await _companyService.GetUserCompanyAccountById(connectedId); // subCompany ise buraya girecek
                }
                else
                {
                    subCompanys = await _companyService.GetUserCompanyAccountByConnectedUserCompanyID(company.UserCompany_ID);

                }
            }
            else
            {
                // Hiç değer gelmemiş veya geçersiz Guid
            }



            // ViewModel
            var viewModel = new CompanyDetailsViewModel
            {
                Company = company,
                CityName = cityName,
                ProvinceName = provinceName,
                PostName = postName,
                TaxName = taxName,
                IsPartnerPassive = isPassive,
                Documents = documents,
                FamilyList = familyList,
                ProductList = products,
                VersionList = versionList,
                TransactionLicenseDetail = transactionLicenseDetailLists,
                TransactionLicenseMaster = transactionLicenseMasterList,
                Is_Partner = isPartner,
                connectedCompany = connectedCompany.value,
                subCompanys = subCompanys?.value ?? new List<GetUserCompanyAccountByFilterResponse.Value>(),
                RegistryPoints = registryPoints ?? new List<GetUserCompanyRegistryPointByUserCompanyIdResponse.Value>()
            };

            return PartialView("CompanyDetails", viewModel);
        }



        public class ProductListRequest
        {
            public Guid Product_Family_ID { get; set; }
            public Guid Product_Register_Type_ID { get; set; }
            public Guid Product_Module_ID { get; set; }
        }
        public async Task<IActionResult> ProductListByDropdownLists(ProductListRequest request)
        {
            var products = await _productService.GetProductsByFamilyRegisterModule(request);

            return Json(new
            {
                Products = products.value
            });
        }
        public async Task<IActionResult> NewSale(string identityNo, string name, string provinceName, string currentVersionListJson, string companyId, string partnerId)
        {
            ViewBag.CompanyId = companyId;
            ViewBag.PartnerId = partnerId;
            var allProduct = await _productService.GetAllProduct();

            var versionList = new List<GetProductVersionByIdRepository.Value>(); 

            var uniqueVersionIds = allProduct.value
                .Select(p => p.Product_Version_ID)
                .Distinct()
                .ToList();

            foreach (var versionId in uniqueVersionIds)
            {
                var version = await _productService.GetProductVersionById(versionId.ToString());
                versionList.Add(version.value);
            }

            ViewBag.Name = name;
            ViewBag.VersionList = versionList;
            ViewBag.IdentityNo = identityNo;
            ViewBag.ProvinceName = provinceName;
            ViewBag.CurrentVersionListJson = currentVersionListJson;

            var registerPointResult = await _companyService.GetUserCompanyRegistryPointByUserCompanyId(companyId);
            List<UserCompanyRegistryPointIdModel> registerPoints = new List<UserCompanyRegistryPointIdModel>();


            if (registerPointResult?.value == null || registerPointResult.value.Count == 0)
            {
                registerPoints.Add(new UserCompanyRegistryPointIdModel
                {
                    UserCompany_Registry_Point_ID = Guid.Empty,
                    UserCompany_Registry_Point_Name = "Merkez"
                });
            }
            else
            {      // ...aksi halde, mevcut listeyi senin yeni modeline dönüştürerek kullan.
                registerPoints = registerPointResult.value
                    .Select(rp => new UserCompanyRegistryPointIdModel
                    {
                        UserCompany_Registry_Point_ID = rp.UserCompany_Registry_Point_ID,
                        UserCompany_Registry_Point_Name = rp.UserCompany_Registry_Point_Name
                    })
                    .ToList();
            }

            ViewBag.RegisterPointList = registerPoints;

            return PartialView("NewSale");

        }
        [HttpGet]
        public async Task<JsonResult> GetVersionDetails(Guid versionId)
        {
            var allProducts = await _productService.GetAllProduct();

            var filteredProducts = allProducts.value
            .Where(p => p.Product_Version_ID == versionId)
            .ToList();

            // 2. Bu ürünlerden unique family ve registerType ID'lerini al
            var uniqueFamilyIds = filteredProducts
                .Select(p => p.Product_Family_ID)
                .Distinct()
                .ToList();

            var uniqueRegisterTypeIds = filteredProducts
                .Select(p => p.Product_Register_Type_ID)
                .Distinct()
                .ToList();

            var uniqueModuleIds = filteredProducts
               .Select(p => p.Product_Module_ID)
               .Distinct()
               .ToList();
            // 3. Family ve RegisterType detaylarını liste olarak getir
            var familyList = new List<GetProductFamilByIdRepository.Value>();
            foreach (var familyId in uniqueFamilyIds)
            {
                var family = await _productService.GetProductFamilyById(familyId.ToString());
                if (family != null && family.succeeded)
                    familyList.Add(family.value);
            }

            var registerTypeList = new List<GetProductRegisterTypeByIdRepository.Value>();
            foreach (var regTypeId in uniqueRegisterTypeIds)
            {
                var registerType = await _productService.GetProductRegisterTypeById(regTypeId.ToString());
                if (registerType != null && registerType.succeeded)
                    registerTypeList.Add(registerType.value);
            }

            var moduleList = new List<GetProductModuleByIdRepository.Value>();
            foreach (var moduleId in uniqueModuleIds)
            {
                var module = await _productService.GetProductModuleById(moduleId.ToString());
                if (module != null && module.succeeded)
                    moduleList.Add(module.value);
            }
            // 4. Modüller varsa burada çekilebilir
            //var moduleList = new List<YourModuleType>(); // Gerekirse doldur

            return Json(new
            {
                families = familyList,
                registerTypes = registerTypeList,
                modules = moduleList
            });
        }
      
        [HttpGet]
        public async Task<IActionResult> GetProvinceByCityId(string CityId)
        {
            if (string.IsNullOrEmpty(CityId))
                return BadRequest("City ID geçersiz.");

            var response = await _sharedService.GetProvinceByCityId(CityId);

            if (response == null || response.value == null)
                return Json(new List<object>());

            return Json(response.value);
        }


        [HttpPost]
        public async Task<IActionResult> CreateUserCompanyRegistryPoint(Guid Organization_ID, Guid UserCompany_ID, string UserCompany_Registry_Point_Code, string UserCompany_Registry_Point_Name)
        {
            try
            {
                var memberId = User.FindFirst("UserId")?.Value;
                Guid createdUserGuid = Guid.Empty;
                Guid.TryParse(memberId, out createdUserGuid);

                var model = new UserCompanyRegistryPointModel
                {
                    Created_User = createdUserGuid,
                    OrganizationId = Organization_ID,
                    CompanyId = UserCompany_ID,
                    UserCompany_Registry_Point_Code = UserCompany_Registry_Point_Code,
                    UserCompany_Registry_Point_Name = UserCompany_Registry_Point_Name,

                };
                var response = await _companyService.CreateUserCompanyRegistryPoint(model);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Hata oluştu: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> TogglePassiveStatus(bool isPassive, string partnerId, string companyId)
        {
            if (string.IsNullOrWhiteSpace(partnerId))
            {
                TempData["Error"] = "Partner ID bulunamadı.";
                return RedirectToAction("CompanyDetails", new { companyId });
            }

            if (isPassive)
            {
                TempData["Info"] = "Şirket zaten pasif durumda.";
                return RedirectToAction("CompanyDetails", new { companyId });
            }

            var updateModel = new UpdatePartnerAccountIsPassiveByPartnerIdRequestModel
            {
                Id = Guid.Parse(partnerId),
                Is_Passive = true
            };

            var response = await _partnerService.UpdatePartnerAccountIsPassiveByPartnerId(updateModel);

            if (response.succeeded)
                TempData["Success_Passive"] = "Şirket başarıyla pasifleştirildi.";
            else
                TempData["Error"] = "Pasifleştirme işlemi başarısız oldu.";

            return RedirectToAction("Index", "Home");


        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var mediaList = await _sharedService.GetMediaByMediaIdAsync(id);

            var file = mediaList?.FirstOrDefault(x => x.Media_ID == id);
            if (file == null || file.Media_Content == null)
                return NotFound("Dosya bulunamadı.");

            return File(file.Media_Content, file.Content_Type ?? "application/octet-stream", file.Media_Name);
        }


        [HttpGet]
        public IActionResult Success()
        {
            return View("_Success");
        }
        [HttpGet]
        public IActionResult Fail()
        {
            return View("_Fail");
        }
        [NonAction]
        public IActionResult Packages()
        {


            return View();
        }
        public IActionResult UserInformation()
        {



            return View();
        }
    }
}