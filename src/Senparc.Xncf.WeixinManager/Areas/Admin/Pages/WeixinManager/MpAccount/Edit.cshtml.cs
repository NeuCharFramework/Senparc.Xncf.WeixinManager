using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.Extensions;
using Senparc.Ncf.Service;
using Senparc.Weixin.MP.Containers;
using Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel;
using Senparc.Xncf.WeixinManager.Domain.Models.DatabaseModel.Dto;
using Senparc.Xncf.WeixinManager.Domain.Models.VD.Admin.WeixinManager;
using System;
using System.Threading.Tasks;

namespace Senparc.Xncf.WeixinManager.Areas.Admin.Pages.WeixinManager
{
    public class MpAccount_EditModel : BaseAdminWeixinManagerModel
    {
        [BindProperty]
        public MpAccountDto MpAccountDto { get; set; }

        private readonly ServiceBase<MpAccount> _mpAccountService;
        public bool IsEdit { get; set; }

        public MpAccount_EditModel(Lazy<XncfModuleService> xncfModuleService,
                                   ServiceBase<MpAccount> mpAccountService) : base(xncfModuleService)
        {
            _mpAccountService = mpAccountService;
        }


        public async Task<IActionResult> OnGetAsync(int id = 0)
        {
            IsEdit = id > 0;
            if (IsEdit)
            {
                var mpAccount = await _mpAccountService.GetObjectAsync(z => z.Id == id);
                if (mpAccount == null)
                {
                    return RenderError("���ں���Ϣ�����ڣ�");
                }

                MpAccountDto = _mpAccountService.Mapper.Map<MpAccountDto>(mpAccount);
            }
            else
            {
                MpAccountDto = new MpAccountDto();
            }
            return Page();
        }

        public async Task<IActionResult> OnGetAjaxAsync(int id = 0)
        {
            var mpAccountDto = new MpAccountDto();
            if (id > 0)
            {
                var mpAccount = await _mpAccountService.GetObjectAsync(z => z.Id == id);
                if (mpAccount == null)
                {
                    return RenderError("���ں���Ϣ�����ڣ�");
                }

                mpAccountDto = _mpAccountService.Mapper.Map<MpAccountDto>(mpAccount);
            }
            return Ok(mpAccountDto);
        }

        public async Task<IActionResult> OnPostAsync(int id = 0)
        {
            IsEdit = id > 0;
            MpAccount mpAccount = null;
            if (IsEdit)
            {
                mpAccount = await _mpAccountService.GetObjectAsync(z => z.Id == id);
                if (mpAccount == null)
                {
                    return RenderError("���ں���Ϣ�����ڣ�");
                }
                _mpAccountService.Mapper.Map(MpAccountDto, mpAccount);
            }
            else
            {
                mpAccount = new MpAccount(MpAccountDto);
            }
            await _mpAccountService.SaveObjectAsync(mpAccount);

            //���½��й��ں�ע��
            await AccessTokenContainer.RegisterAsync(mpAccount.AppId, mpAccount.AppSecret, $"{mpAccount.Name}-{mpAccount.Id}");

            try
            {
                //������ȡ AccessToken
                await AccessTokenContainer.GetAccessTokenAsync(mpAccount.AppId, true);
            }
            catch (Exception ex)
            {
                return Ok(new { id = mpAccount.Id, uid = Uid, msg = "�˺��Ѿ���ӣ��� AppId �� Secret �����޷��������������飡" });
            }


            //return RedirectToPage("./Edit", new { id = mpAccount.Id, uid = Uid });
            return Ok(new { id = mpAccount.Id, uid = Uid });
        }

        public async Task<IActionResult> OnPostAjaxAsync([FromBody] MpAccountDto mpAccountDto)
        {
            MpAccount mpAccount = null;
            if (mpAccountDto.Id > 0)
            {
                mpAccount = await _mpAccountService.GetObjectAsync(z => z.Id == mpAccountDto.Id);
                if (mpAccount == null)
                {
                    return RenderError("���ں���Ϣ�����ڣ�");
                }

                mpAccountDto.AddTime = mpAccount.AddTime;
                _mpAccountService.Mapper.Map(mpAccountDto, mpAccount);
                mpAccount.LastUpdateTime = DateTime.Now;
            }
            else
            {
                mpAccount = new MpAccount(mpAccountDto);
            }
            await _mpAccountService.SaveObjectAsync(mpAccount);

            //���½��й��ں�ע��
            await AccessTokenContainer.RegisterAsync(mpAccount.AppId, mpAccount.AppSecret, $"{mpAccount.Name}-{mpAccount.Id}");

            try
            {
                //������ȡ AccessToken
                await AccessTokenContainer.GetAccessTokenAsync(mpAccount.AppId, true);
            }
            catch (Exception ex)
            {
                return Ok(new { id = mpAccount.Id, uid = Uid, msg = "�˺��Ѿ���ӣ��� AppId �� Secret �����޷��������������飡" });
            }


            //return RedirectToPage("./Edit", new { id = mpAccount.Id, uid = Uid });
            return Ok(new { id = mpAccount.Id, uid = Uid });
        }
    }
}
