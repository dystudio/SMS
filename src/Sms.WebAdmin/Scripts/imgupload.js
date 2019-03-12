////默认为单图上传
//var uploadType = 0;
////触发上传的按钮对象
//var uploadObj = null;

//$(function () {

//    $("#imgUpload").ajaxForm(function (obj) {
//        if (obj.Status) {
//            var $athis = $(uploadObj);
//            var $_moreImg = $athis.next(".thumbnails");
//            var $_input = $athis.prevAll("input[type='hidden']");
//            var newImg = '<li class="span2"> <a href="javascript:" title="点击放大"  class="thumbnail"> <img src="/Upload/Image/' + obj.Url + '" alt=""> </a> <div class="actions"> <a title="移除" href="javascript:" class="op"><i class="icon-remove icon-white"></i></a> </div> </li>';
//            if (uploadType == 1) {//多图列表
//                $_moreImg.append(newImg);
//                var oldValue = $_input.val();//拿到旧值
//                if (oldValue && oldValue != "") {
//                    oldValue += "$" + obj.Url;
//                }
//                else {
//                    oldValue += obj.Url;
//                }
//                //设值
//                $_input.val(oldValue);
//            }
//            else if (uploadType == 0) {//单图
//                $_moreImg.html(newImg);
//                $_input.val(obj.Url);
//            }
//            BindEvent();
//        }
//        else {
//            show_message(false, obj.MsgText, null);

//        }
//    });
//    //初始化事件绑定
//    BindEvent();
//});

//绑定通用事件
function BindEvent() {
    //多图删除事件
    $(".thumbnails .op").unbind("click").click(function () {
        if (confirm("确认移除该图片吗？")) {
            var base = $(this).parents(".thumbnails");
            $(this).parents("li").remove();
            //删除隐藏值
            var $_input = base.prevAll("input[type='hidden']");
            var newValue = "";
            base.children("li").each(function (i) {
                var excision = i == 0 ? "" : "$";
                newValue += excision + $(this).find("img").attr("src");
            });
            $_input.val(newValue);
        }
    });

    //openView();
}


var defaultOpts = {
    multiple: false,
    maxCount: 3,
    maxSize: 2,//M
    ext: 'png,jpeg'
};

$.fn.extend({
    uploadImage: function (option) {
        var _this = $(this)[0];
        var $athis = $(_this);
        option = $.extend(option, defaultOpts);
        var extArry = option.ext.split(',');
        var accept = [];
        for (var i = 0; i < extArry.length; i++) {
            accept.push("image/" + extArry[i]);
        }
        var formid = "upload_from_" + _this.id, btnid = "upload_btn_" + _this.id;
        var init = function () {
            $("body").append("<form id=\"" + formid + "\" action=\"/FileHandler/Image\" enctype=\"multipart/form-data\" method=\"post\" style=\"display:none\"> <input type=\"file\" accept=\"" + accept.join(',') + "\" id=\"" + btnid + "\" name=\"filedata\" " + (option.multiple ? "multiple=\"multiple\"" : "") + " /> </form>");
            var contrl = $athis.parent();
            contrl.append("<a href=\"javascript:\" class=\"btn btn-info\" onclick=\"$(\"#" + btnid + "\").click();\"><i class=\"icon-picture icon-white\"></i>&nbsp 选择图片</a><ul class=\"thumbnails\">");
            if (_this.value && _this.value.length > 0) {
                var imgUrls = _this.value.split("$");
                for (var i = 0; i < imgUrls.length; i++) {
                    contrl.append('<li class="span2"> <a href="' + imgUrls[i] + '" title="点击查看原图" target="_blank"  class="thumbnail"> <img src="' + imgUrls[i] + '" alt=""> </a> <div class="actions"> <a title="移除" href="javascript:" class="op"><i class="icon-remove"></i></a> </div> </li>');
                }
            }
            contrl.append("</ul>");
            BindEvent();
        }
        init();
        //var $_input = $athis.prevAll("input[type='hidden']");
        $("#" + formid).ajaxForm(function (obj) {
            if (obj.Status === true) {
                var $_moreImg = $athis.parent().find(".thumbnails");
                var imgUrls = obj.Url.split("$");
                for (var i = 0; i < imgUrls.length; i++) {
                    var url = imgUrls[i];
                    var newImg = '<li class="span2"> <a href="' + url + '" title="点击查看原图" target="_blank"  class="thumbnail"> <img src="' + url + '" alt=""> </a> <div class="actions"> <a title="移除" href="javascript:" class="op"><i class="icon-remove"></i></a> </div> </li>';
                    if (option.multiple) {//多图列表
                        $_moreImg.append(newImg);
                        var oldValue = $athis.val();//拿到旧值
                        if (oldValue && oldValue != "") {
                            oldValue += "$" + url;
                        }
                        else {
                            oldValue += url;
                        }
                        //设值
                        $athis.val(oldValue);
                    }
                    else {//单图
                        $_moreImg.html(newImg);
                        $athis.val(url);
                    }
                }
                BindEvent();
            }
            else {
                show_message(false, obj.MsgText, null);
            }
        });
        $("#" + btnid).bind("change", function (e) {
            for (var i = 0; i < e.currentTarget.files.length; i++) {
                var file = e.currentTarget.files[i];
                if (accept.indexOf("image/*") == -1 && accept.indexOf(file.type) == -1) {
                    alert("只允许上传" + option.ext + "格式的图片！");
                    return false;
                }
                if (file.size > option.maxSize * 1024 * 1024) {
                    alert("图片超过最大限制，最大为" + option.maxSize + "M！");
                    return false;
                }
                if (option.multiple == true && $_moreImg.find("li").length + e.currentTarget.files.length > option.maxCount) {
                    alert("最多上传" + option.maxCount + "张图片！");
                    return false;
                }
            }
            $("#" + formid).submit();
        });
    }
});