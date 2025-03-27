using IAChatDB.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;
using MudBlazor;

namespace IAChatDB.Components.ChatIA;

public partial class ChatIA
{
    [Parameter] public bool ChatLoading { get; set; }
    [Parameter] public bool ChatActivo { get; set; }
    [Parameter] public ChatHistory ChatData { get; set; } 
    [Parameter] public bool _shouldScroll { get; set; }
    [Parameter] public FormModel Model { get; set; }
    [Parameter] public EventCallback OnSend { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = null!;
    private IJSObjectReference? _module;

    private ElementReference _scrollContainer;

    private Color Color = Color.Success;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/scrollHelper.js");
        }

        if (_shouldScroll)
        {
            _shouldScroll = false;
            await _module!.InvokeVoidAsync("scrollToBottom", _scrollContainer);
        }
    }

    private async Task HandleOnIAChat()
    {
        if (!string.IsNullOrWhiteSpace(Model.PromptIAChat))
        {
            await OnSend.InvokeAsync();
        }
    }

    private async Task HandleClear()
    {
        await OnClear.InvokeAsync();
    }

    private string ConvertMarkdownToHtml(string markdown)
    {
        var pipeline = new Markdig.MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        return Markdig.Markdown.ToHtml(markdown, pipeline);
    }
}