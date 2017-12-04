﻿$.fn.extend({
    animateOut: function (animationName) {
        var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
        this.addClass('animated ' + animationName).one(animationEnd, function () {
            $(this).removeClass('animated ' + animationName);
            $(this).remove();
        });
    }
});
$.fn.extend({
    animateIn: function (animationName) {
        var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
        this.addClass('animated ' + animationName).one(animationEnd, function () {
            $(this).removeClass('animated ' + animationName);
        });
    }
});
function ChangeOrder() {
    $('section').droppable('disable');
    $('.component-container').droppable('disable');

    $('#page-wrapper').toggleClass("zoom");
    $('#page-wrapper').toggleClass("change-order");
    $('#page-wrapper.change-order').sortable({ placeholder: "ui-state-highlight", helper: 'clone' });

    $('.admin-wrapper .button').popover('hide');
    $('.page-admin-wrapper').fadeOut();
    $('.action-container.section-order').fadeIn();

    $('.panel-overlay').slideUp(300);
    $('.panel-overlay').removeClass('visible');
}
function SaveOrder() {
    var associationList = [];
    var orderId = 1;
    $("#page-wrapper .sortable").each(function (index) {
        var associationId = $(this).attr("data-association");
        associationList.push(orderId + "-" + associationId);
        orderId += 1;
    });
    $('#order-list').val(associationList);
    $('#order-submit').click();
}
function ApplySectionControls() {
    $('.section-wrapper .action-container').remove();

    var spinnerMarkup = '<div class=&quot;dynamic-popover-content&quot;><div class=&quot;spinner&quot;></div></div>';

    var sectionButtonsTemplate = '<div class="action-container absolute">' +
        '<a class="action edit-markup launch-modal hidden-xs" data-title="Edit Markup" href="/PageBuilder/Section/Markup?pageSectionId=<sectionId>"><span class="fa fa-code"></span></a>' +
        '<a class="action launch-modal hidden-xs" data-title="Backup or Restore a Section" href="/PageBuilder/Section/Restore?pageSectionId=<sectionId>"><span class="fa fa-clock-o"></span></a>' +
        '<a class="action launch-popover" data-title="Clone Section" data-toggle="popover" data-placement="bottom" data-trigger="click" data-html="true" data-content="<spinner>" data-url="/PageBuilder/Section/Clone?pageAssociationId=<associationId>"><span class="fa fa-clone"></span></a>' +
        '<a class="action edit-section launch-modal" data-title="Edit Section" href="/PageBuilder/Section/EditSection?pageAssociationId=<associationId>"><span class="fa fa-cog"></span></a>' +

        '</div > ';

    sectionButtonsTemplate = sectionButtonsTemplate.replace(/<spinner>/g, spinnerMarkup);

    $(".section-wrapper").each(function (index) {
        var sectionId = $(this).attr("data-section");
        var associationId = $(this).attr("data-association");

        var sectionButtonsMarkup = sectionButtonsTemplate.replace(/<sectionId>/g, sectionId);
        sectionButtonsMarkup = sectionButtonsMarkup.replace(/<associationId>/g, associationId);

        $(this).append(sectionButtonsMarkup);
    });

    $('.partial-wrapper .action-container').remove();
    var partialButtonsTemplate = '<div class="action-container absolute"><a class="action edit-partial launch-modal" data-title="Edit Partial" href="/PageBuilder/Section/EditPartial?pageAssociationId=<associationId>"><span class="fa fa-cog"></span></a></div>';
    $(".partial-wrapper").each(function (index) {
        var associationId = $(this).attr("data-association");
        partialButtonsMarkup = partialButtonsTemplate.replace(/<associationId>/g, associationId);

        $(this).append(partialButtonsMarkup);
    });
}

$(document).ready(function () {
    InitialiseEditor();
});

function InitialiseEditor() {
    ApplySectionControls();

    for (var i = tinymce.editors.length - 1; i > -1; i--) {
        var ed_id = tinymce.editors[i].id;
        tinyMCE.execCommand("mceRemoveEditor", true, ed_id);
    }

    $('.admin .section-wrapper .component-container, .admin .section-wrapper .widget-wrapper:not(.video), .admin .section-wrapper section .widget-wrapper.video, .admin .section-wrapper section .image').unbind();

    $(".admin .section-wrapper .component-container, .admin .section-wrapper .widget-wrapper:not(.video)").click(function (event) {
        if (event.target !== this) return;
        var elementId = event.target.id;
        var sectionId = ExtractSectionId($(this));

        var href = "/PageBuilder/Component/EditContainer?pageSectionId=" + sectionId + "&elementId=" + elementId + "&elementType=div";
        showModalEditor("Edit Container Properties", href);
    });
    $(".admin .section-wrapper section .image").click(function (event) {
        var elementId = event.target.id;
        var sectionId = ExtractSectionId($(this));
        var elementType = "div";

        if ($(this).is('img')) {
            elementType = "img";
        }

        var href = "/PageBuilder/Component/EditImage?pageSectionId=" + sectionId + "&elementId=" + elementId + "&elementType=" + elementType;
        showModalEditor("Edit Image Properties", href);
    });
    $(".admin .section-wrapper section .widget-wrapper.video").click(function (event) {
        var elementId = event.target.id;
        var sectionId = ExtractSectionId($(this));

        var videoPlayerElementId = $(this).find('iframe').first().attr("id");

        var href = "/PageBuilder/Component/EditVideo?pageSectionId=" + sectionId + "&widgetWrapperelementId=" + elementId + "&videoPlayerElementId=" + videoPlayerElementId;
        showModalEditor("Edit Video Properties", href);
    });

    tinymce.init({
        selector: '.admin .section-wrapper section p, .admin .section-wrapper section h1, .admin .section-wrapper section h2, .admin .section-wrapper section h3, .admin .section-wrapper section h4, .admin .section-wrapper section code, .admin .section-wrapper section a, .admin .section-wrapper section .btn',
        menubar: false, inline: true,
        plugins: ['advlist textcolor colorpicker link'],

        toolbar: 'bold italic underline | link | forecolor backcolor | delete',
        setup: function (ed) {
            ed.addButton('delete', { icon: 'trash', onclick: function () { DeleteInlineComponent(tinyMCE.activeEditor.id); } }),
                ed.on('blur', function (e) { EditInlineText(tinyMCE.activeEditor.id, ed.getContent()); });
        }
    });
    tinymce.init({
        selector: '.admin .section-wrapper section .freestyle',
        menubar: true, inline: true,
        plugins: ['advlist autolink lists link image charmap print preview anchor searchreplace visualblocks code fullscreen insertdatetime media table contextmenu paste textcolor colorpicker'],
        toolbar: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | forecolor backcolor | bullist numlist outdent indent | link image | delete',
        setup: function (ed) {
            ed.addButton('delete', { icon: 'trash', onclick: function () { DeleteInlineComponent(tinyMCE.activeEditor.id); } }),
                ed.on('blur', function (e) { EditInlineFreestyle(tinyMCE.activeEditor.id, ed.getContent()); });
        }
    });
}
function InitialiseContainer(elementId) {
    $("#" + elementId).droppable({
        tolerance: "intersect", activeClass: "ui-state-default", hoverClass: "ui-state-hover", greedy: "true", drop: function (event, ui) { DropComponent(this, event, ui); }
    });
}
function InitialiseDroppables() {
    $("section").droppable({
        accept: function () { return PreventAppDrawerDrop(); },
        tolerance: "pointer",
        activeClass: "ui-state-default",
        hoverClass: "ui-state-hover",
        drop: function (event, ui) { DropComponent(this, event, ui); }
    });
    $(".component-container").droppable({
        accept: function () { return PreventAppDrawerDrop(); },
        tolerance: "pointer",
        activeClass: "ui-state-default",
        hoverClass: "ui-state-hover",
        greedy: "true",
        drop: function (event, ui) { DropComponent(this, event, ui); }
    });
}
function EditInlineText(editorId, editorContent) {
    var elementId = editorId;
    var sectionId = ExtractSectionId($('#' + editorId));

    editorContent = RemoveTinyMCEAttributes(editorContent);

    var dataParams = { "pageSectionId": sectionId, "elementId": elementId, "elementHtml": editorContent };
    $.ajax({
        data: dataParams,
        type: 'POST',
        cache: false,
        url: '/PageBuilder/Component/Edit',
        success: function (data) { if (data.State === false) { alert("Error: The Page has lost synchronisation. Reloading Page..."); location.reload(); } }
    });
}
function RemoveTinyMCEAttributes(htmlContent) {
    htmlContent = htmlContent.replace('mce-content-body', '');
    htmlContent = htmlContent.replace('position: relative;', '');
    htmlContent = htmlContent.replace('contenteditable="true" ', '');

    return htmlContent;
}
function EditInlineFreestyle(editorId, editorContent) {
    var elementId = editorId;
    var sectionId = ExtractSectionId($('#' + editorId));

    var dataParams = { "pageSectionId": sectionId, "elementId": elementId, "elementHtml": editorContent };
    $.ajax({
        data: dataParams,
        type: 'POST',
        cache: false,
        url: '/PageBuilder/Component/Edit',
        success: function (data) { if (data.State === false) { alert("Error: The Page has lost synchronisation. Reloading Page..."); location.reload(); } }
    });
}
function EditInlineAnchor(editorId, editorContent) {
    var elementId = editorId;
    var sectionId = ExtractSectionId($('#' + editorId));

    var href = $('#' + elementId).attr("href");
    var target = $('#' + elementId).attr("target");

    var dataParams = { "pageSectionId": sectionId, "elementId": elementId, "elementHtml": editorContent, "elementHref": href, "elementTarget": target };
    $.ajax({
        data: dataParams,
        type: 'POST',
        cache: false,
        url: '/PageBuilder/Component/Link',
        success: function (data) { if (data.State === false) { alert("Error: The Page has lost synchronisation. Reloading Page..."); location.reload(); } }
    });
}
function PreventAppDrawerDrop() {
    if (window.innerHeight < 701 && window.innerWidth < 601) {
        return true;
    }

    var tray = $("#component-panel").offset();
    var trayWidth = $("#component-panel").width();
    var trayHeight = $("#component-panel").height();
    var trayTop = tray.top - $(document).scrollTop();

    var x = event.clientX;
    var y = event.clientY;
    if (x >= tray.left && x <= tray.left + trayWidth && y >= trayTop && y <= trayTop + trayHeight) {
        return false;
    }

    return true;
}
function DropComponent(control, event, ui) {
    var newElement = $(ui.draggable).clone();

    var componentStamp = new Date().valueOf();
    var sectionId = ExtractSectionId($(control));
    var newElementId = newElement.attr("id");

    newElementId = newElementId.replace('<sectionId>', sectionId);
    newElementId = newElementId.replace('<componentStamp>', componentStamp);
    newElement.attr("id", newElementId);

    $(control).append(newElement);

    $('#' + newElementId).removeClass("ui-draggable");
    $('#' + newElementId).removeClass("ui-draggable-handle");
    $('#' + newElementId).unbind();
    $('#' + newElementId).animateIn('bounce');

    ReplaceChildTokens(newElementId, sectionId, componentStamp);

    var newElementContent = $('#' + newElementId)[0].outerHTML;

    newElementContent = newElementContent.replace(/&lt;componentStamp&gt;/g, componentStamp);
    newElementContent = newElementContent.replace(/&lt;sectionId&gt;/g, sectionId);

    newElementContent = newElementContent.replace(/<componentStamp>/g, componentStamp);
    newElementContent = newElementContent.replace(/<sectionId>/g, sectionId);

    $('#' + newElementId).replaceWith(newElementContent);

    InitialiseEditor();
    InitialiseWidgets();

    if (newElement.hasClass("component-container")) {
        InitialiseContainer(newElementId);
    }

    var dataParams = { "pageSectionId": sectionId, "containerElementId": $(control).attr("id"), "elementBody": newElementContent };
    $.ajax({
        data: dataParams,
        type: 'POST',
        cache: false,
        url: '/PageBuilder/Component/Add',
        success: function (data) { if (data.State === false) { alert("Error: The Page has lost synchronisation. Reloading Page..."); location.reload(); } }
    });
}
function DeleteInlineComponent(editorId) {
    var elementId = editorId;
    var sectionId = ExtractSectionId($('#' + editorId));

    tinymce.execCommand('mceRemoveControl', true, editorId);

    var dataParams = { "pageSectionId": sectionId, "elementId": elementId };
    $('#' + elementId).animateOut('flipOutX');
    $.ajax({
        data: dataParams,
        type: 'POST',
        cache: false,
        url: '/PageBuilder/Component/Delete',
        success: function (data) { if (data.State === false) { alert("Error: The Page has lost synchronisation. Reloading Page..."); location.reload(); } }
    });
}

function ExtractSectionId(element) {
    var elementId = $(element).attr("id");

    if (elementId !== undefined) {
        var elementParts = elementId.split('-');
        var sectionId = elementParts[elementParts.length - 1];
        return sectionId;
    }
}
function ReplaceChildTokens(parentElementId, sectionId, componentId) {
    $('#' + parentElementId).children().each(function () {
        var childId = $(this).attr("id");

        if (childId !== undefined) {
            childId = childId.replace("<sectionId>", sectionId);
            childId = childId.replace("<componentStamp>", componentId);

            $(this).attr("id", childId);

            ReplaceChildTokens(childId, sectionId, componentId);
        }
    });
}
function ReloadSection(pageSectionId) {
    $('#spinner-wrapper').show();
    $.ajax({
        data: { "pageSectionId": pageSectionId },
        type: 'GET',
        cache: false,
        url: '/PageBuilder/Section/Reload',
        success: function (data) {
            $('#section-wrapper-' + pageSectionId).empty();
            $('#section-wrapper-' + pageSectionId).append(data);

            InitialiseEditor();
            InitialiseWidgets();
            InitialiseDroppables();

            $('#spinner-wrapper').hide();
        }
    });
}