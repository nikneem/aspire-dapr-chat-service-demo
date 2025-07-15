      let connection = null;
      let currentUser = null;
      let config = null;

      // Get configuration from server endpoint
      async function loadConfiguration() {
        try {
          const response = await fetch("/api/config");
          if (response.ok) {
            config = await response.json();
            updateConfigDisplay();
            updateClientSettings();
            return true;
          } else {
            // Fallback to default configuration for development
            config = {
              membersApiUrl: "http://localhost:5129",
              messagesApiUrl: "http://localhost:5227",
              realtimeApiUrl: "http://localhost:5206",
              client: {
                title: "AspireChat - Fallback",
                description: "Chat application with fallback configuration",
                debug: true
              }
            };
            updateConfigDisplay();
            updateClientSettings();
            addSystemMessage(
              "Using fallback configuration - ensure Aspire services are running"
            );
            return false;
          }
        } catch (error) {
          console.error("Failed to load configuration:", error);
          // Fallback configuration
          config = {
            membersApiUrl: "http://localhost:5129",
            messagesApiUrl: "http://localhost:5227",
            realtimeApiUrl: "http://localhost:5206",
            client: {
              title: "AspireChat - Fallback",
              description: "Chat application with fallback configuration",
              debug: true
            }
          };
          updateConfigDisplay();
          updateClientSettings();
          addSystemMessage("Failed to load configuration - using defaults");
          return false;
        }
      }

      function updateConfigDisplay() {
        const configDiv = document.getElementById("configInfo");
        configDiv.innerHTML = `
          <strong>Service Configuration:</strong><br>
          Members API: ${config.membersApiUrl}<br>
          Messages API: ${config.messagesApiUrl}<br>
          Realtime API: ${config.realtimeApiUrl}<br>
          Environment: ${config.client?.debug ? 'Development' : 'Production'}
        `;
      }

      function updateClientSettings() {
        // Update page title if provided
        if (config.client?.title) {
          document.title = config.client.title;
        }

        // Add debug information if in debug mode
        if (config.client?.debug) {
          console.log('Debug mode enabled');
          console.log('Configuration:', config);
        }
      }

      async function registerMember() {
        const name = document.getElementById("nameInput").value.trim();

        if (!name) {
          alert("Please enter your name");
          return;
        }

        try {
          // Register member with Members API
          const response = await fetch(`${config.membersApiUrl}/members`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({ name }),
          });

          if (!response.ok) {
            throw new Error("Failed to register member");
          }

          const member = await response.json();
          currentUser = member;

          // Connect to SignalR
          await connectToSignalR();

          document.getElementById("nameInput").disabled = true;
          document.getElementById("registerBtn").disabled = true;
          document.getElementById("messageInput").disabled = false;
          document.getElementById("sendBtn").disabled = false;

          addSystemMessage(`Registered as ${member.name}`);
        } catch (error) {
          alert("Error registering: " + error.message);
          console.error("Registration error:", error);
          addSystemMessage(
            `Error registering: ${error.message}. Please check if the services are running.`
          );
        }
      }

      async function connectToSignalR() {
        connection = new signalR.HubConnectionBuilder()
          .withUrl(`${config.realtimeApiUrl}/chathub`)
          .build();

        connection.on(
          "ReceiveMessage",
          (id, content, senderId, senderName, sentAt) => {
            addMessage(senderName, content, sentAt);
          }
        );

        connection.on("MemberJoined", (id, name, joinedAt) => {
          addSystemMessage(`${name} joined the chat`);
        });

        connection.on("MemberLeft", (id, name, leftAt) => {
          addSystemMessage(`${name} left the chat`);
        });

        connection.onclose(async () => {
          updateStatus("disconnected");
          document.getElementById("messageInput").disabled = true;
          document.getElementById("sendBtn").disabled = true;
        });

        try {
          await connection.start();
          updateStatus("connected");
          await connection.invoke(
            "JoinChatRoom",
            currentUser.id,
            currentUser.name
          );
        } catch (error) {
          console.error("SignalR connection error:", error);
          updateStatus("disconnected");
          addSystemMessage(
            `SignalR connection failed: ${error.message}. Please check if the Realtime API is running.`
          );
        }
      }

      async function sendMessage() {
        const messageInput = document.getElementById("messageInput");
        const content = messageInput.value.trim();

        if (!content || !currentUser) {
          return;
        }

        try {
          // Send message via Messages API
          const response = await fetch(`${config.messagesApiUrl}/messages`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              content: content,
              senderId: currentUser.id,
            }),
          });

          if (!response.ok) {
            throw new Error("Failed to send message");
          }

          messageInput.value = "";

          // Update member activity
          try {
            await fetch(
              `${config.membersApiUrl}/members/${currentUser.id}/activity`,
              {
                method: "PUT",
              }
            );
          } catch (activityError) {
            console.warn("Failed to update activity:", activityError);
          }
        } catch (error) {
          alert("Error sending message: " + error.message);
          console.error("Send message error:", error);
          addSystemMessage(`Error sending message: ${error.message}`);
        }
      }

      function addMessage(senderName, content, sentAt) {
        const chatContainer = document.getElementById("chatContainer");
        const messageDiv = document.createElement("div");
        messageDiv.className = "message";

        const headerDiv = document.createElement("div");
        headerDiv.className = "message-header";
        headerDiv.textContent = `${senderName} - ${new Date(
          sentAt
        ).toLocaleTimeString()}`;

        const contentDiv = document.createElement("div");
        contentDiv.className = "message-content";
        contentDiv.textContent = content;

        messageDiv.appendChild(headerDiv);
        messageDiv.appendChild(contentDiv);
        chatContainer.appendChild(messageDiv);
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }

      function addSystemMessage(content) {
        const chatContainer = document.getElementById("chatContainer");
        const messageDiv = document.createElement("div");
        messageDiv.className = "message system-message";

        const contentDiv = document.createElement("div");
        contentDiv.className = "message-content";
        contentDiv.textContent = content;

        messageDiv.appendChild(contentDiv);
        chatContainer.appendChild(messageDiv);
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }

      function updateStatus(status) {
        const statusDiv = document.getElementById("status");
        statusDiv.className = `status ${status}`;
        statusDiv.textContent =
          status === "connected" ? "Connected" : "Disconnected";
      }

      // Allow sending message with Enter key
      document
        .getElementById("messageInput")
        .addEventListener("keypress", function (e) {
          if (e.key === "Enter") {
            sendMessage();
          }
        });

      // Load recent messages on page load
      async function loadRecentMessages() {
        try {
          const response = await fetch(
            `${config.messagesApiUrl}/messages/recent?count=10`
          );
          if (response.ok) {
            const messages = await response.json();
            messages.forEach((message) => {
              addMessage(message.senderName, message.content, message.sentAt);
            });
          }
        } catch (error) {
          console.error("Error loading recent messages:", error);
          addSystemMessage(
            "Unable to load recent messages. Please ensure the Messages API is running."
          );
        }
      }

      // Test connectivity on page load
      async function testConnectivity() {
        addSystemMessage("Testing service connectivity...");

        // Test Members API
        try {
          await fetch(`${config.membersApiUrl}/`, {
            method: "HEAD",
            timeout: 5000,
          });
          addSystemMessage("✓ Members API is accessible");
        } catch (error) {
            console.log("Members API connectivity test failed:", error);
          addSystemMessage(
            "✗ Members API is not accessible - check if service is running"
          );
        }

        // Test Messages API
        try {
          await fetch(`${config.messagesApiUrl}/`, {
            method: "HEAD",
            timeout: 5000,
          });
          addSystemMessage("✓ Messages API is accessible");
        } catch (error) {
            console.log("Messages API connectivity test failed:", error);
          addSystemMessage(
            "✗ Messages API is not accessible - check if service is running"
          );
        }

        // Test loading recent messages
        await loadRecentMessages();
      }

      // Initialize when page loads
      window.addEventListener("load", async () => {
        await loadConfiguration();
        await testConnectivity();
      });