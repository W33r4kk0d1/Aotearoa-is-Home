
document.addEventListener("DOMContentLoaded", function () {
    const widget = document.getElementById("chatbotWidget");

    if (!widget) {
        return;
    }

    const toggleButton = document.getElementById("chatbotToggle");
    const panel = document.getElementById("chatbotPanel");
    const closeButton = document.getElementById("chatbotClose");
    const form = document.getElementById("chatForm");
    const input = document.getElementById("messageInput");
    const messages = document.getElementById("chatMessages");
    const sendButton = document.getElementById("chatSendButton");
    const status = document.getElementById("chatStatus");

    const responseUrl = widget.dataset.responseUrl;

    function openChat() {
        panel.hidden = false;
        panel.setAttribute("aria-hidden", "false");
        toggleButton.setAttribute("aria-expanded", "true");
        toggleButton.setAttribute("aria-label", "Close chatbot");

        input.focus();
    }

    function closeChat() {
        panel.hidden = true;
        panel.setAttribute("aria-hidden", "true");
        toggleButton.setAttribute("aria-expanded", "false");
        toggleButton.setAttribute("aria-label", "Open chatbot");
    }

    function toggleChat() {
        if (panel.hidden) {
            openChat();
        } else {
            closeChat();
        }
    }

    // Open or close using the floating button or navbar icons.
    document.querySelectorAll("[data-chatbot-toggle]")
        .forEach(function (button) {
            button.addEventListener("click", function (event) {
                event.preventDefault();
                toggleChat();
            });
        });

    // Also support the main floating button.
    toggleButton.addEventListener("click", toggleChat);

    closeButton.addEventListener("click", closeChat);

    // Close when clicking outside the chatbot.
    document.addEventListener("click", function (event) {
        if (
            !panel.hidden &&
            !widget.contains(event.target) &&
            !event.target.closest("[data-chatbot-toggle]")
        ) {
            closeChat();
        }
    });

    // Close with Escape.
    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && !panel.hidden) {
            closeChat();
            toggleButton.focus();
        }
    });

    // Add a message without interpreting it as HTML.
    function addMessage(sender, text, isUser) {
        const message = document.createElement("div");
        message.className = isUser
            ? "chat-message user-message"
            : "chat-message assistant-message";

        const label = document.createElement("div");
        label.className = "chat-message-label";
        label.textContent = sender;

        const bubble = document.createElement("div");
        bubble.className = "chat-message-bubble";
        bubble.textContent = text;

        message.appendChild(label);
        message.appendChild(bubble);
        messages.appendChild(message);

        messages.scrollTop = messages.scrollHeight;
    }

    form.addEventListener("submit", async function (event) {
        event.preventDefault();

        const message = input.value.trim();

        if (!message || sendButton.disabled) {
            return;
        }

        const tokenInput = form.querySelector(
            'input[name="__RequestVerificationToken"]'
        );

        if (!tokenInput) {
            status.textContent =
                "Security token missing. Please refresh the page.";
            return;
        }

        addMessage("You", message, true);

        input.value = "";
        sendButton.disabled = true;
        status.textContent = "Assistant is preparing a response...";

        try {
            const response = await fetch(responseUrl, {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/x-www-form-urlencoded;charset=UTF-8",
                    "RequestVerificationToken": tokenInput.value
                },
                
                body: new URLSearchParams({
                    message: message,
                    __RequestVerificationToken: tokenInput.value
                })
            });

            
            if (!response.ok) {
                const errorDetails = await response.text();

                console.error("Chatbot request failed:", {
                    url: response.url,
                    status: response.status,
                    statusText: response.statusText,
                    responseBody: errorDetails
                });

                throw new Error(
                    `Chatbot request failed (${response.status}). ` +
                    "Please check the browser console for details."
                );
            }


            const data = await response.json();

            if (typeof data.response !== "string") {
                throw new Error("The chatbot returned an unexpected response.");
            }

            addMessage("Assistant", data.response, false);
            status.textContent = "";

        } catch (error) {
            status.textContent = "";

            addMessage(
                "Assistant",
                error.message ||
                    "Something went wrong. Please try again.",
                false
            );
        } finally {
            sendButton.disabled = false;
            input.focus();
        }
    });
});
