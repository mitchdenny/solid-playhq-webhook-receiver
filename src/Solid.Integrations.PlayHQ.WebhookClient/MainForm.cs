using Azure.Messaging.EventHubs.Consumer;
using Solid.Integrations.PlayHQ.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Solid.Integrations.PlayHQ.WebhookClient
{
    public partial class MainForm : Form
    {
        private BindingList<EventEntry> eventEntries = new BindingList<EventEntry>();
        private CancellationTokenSource? cts;

        public MainForm()
        {
            InitializeComponent();
        }


        private async Task<Configuration> LoadConfigurationAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open);
            var configuration = await JsonSerializer.DeserializeAsync<Configuration>(stream);
            return configuration!;
        }

        private async Task<string> GetConnectionStringAsync(Uri endpoint, Guid tenantId, Guid playingSurfaceId, string secret, CancellationToken cancellationToken)
        {
            var nonce = Guid.NewGuid();
            var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(); // allow for 5 minutes drift
            var signedContent = $"{tenantId} {playingSurfaceId} {nonce} {expiry}";
            var signature = SignatureHelper.GenerateSignature(signedContent, secret);

            using var client = new HttpClient();
            
            var requestUrl = $"{endpoint}tenants/{tenantId}/playing-surfaces/{playingSurfaceId}/config?nonce={nonce}&expiry={expiry}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Signature", signature);
            var response = await client.SendAsync(request);
            
            var playingSurfaceConfiguration = await response.Content.ReadFromJsonAsync<PlayingSurfaceConfiguration>(new JsonSerializerOptions(), cancellationToken);

            return playingSurfaceConfiguration!.ConnectionString;
        }

        private async Task ReceiveEventsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var configuration = await LoadConfigurationAsync(configurationPathTextBox.Text);
                var connectionString = await GetConnectionStringAsync(
                    configuration.Endpoint,
                    configuration.TenantId,
                    configuration.PlayingSurfaceId,
                    configuration.Secret,
                    cancellationToken
                    );

                var client = new EventHubConsumerClient(EventHubConsumerClient.DefaultConsumerGroupName, connectionString);
                var events = client.ReadEventsAsync(false, new ReadEventOptions(), cancellationToken);

                await foreach (var @event in events)
                {
                    var payload = @event.Data.EventBody.ToString();
                    await File.WriteAllTextAsync(configuration.OutputPath, payload, cancellationToken);

                    var entry = new EventEntry(DateTimeOffset.UtcNow, payload);

                    this.Invoke(() =>
                    {
                        eventEntries.Add(entry);
                    });
                }
            }
            catch (TaskCanceledException)
            {
                // Swallow!
            }
            catch (Exception ex)
            {
                this.Invoke(() =>
                {
                    cts?.Cancel();

                    MessageBox.Show(
                        ex.ToString(),
                        "Error occured receiving events",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                });
            }
        }

        private void StartReceivingEvents()
        {
            cts = new CancellationTokenSource();
            Task.Run(() => ReceiveEventsAsync(cts.Token));
        }

        private void StopReceivingEvents()
        {
            cts?.Cancel();
        }


        private void startButton_Click(object sender, EventArgs e)
        {
            StartReceivingEvents();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PlayHQ Configuration|*.playhqconfig";
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.configurationPathTextBox.Text = dialog.FileName;
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            StopReceivingEvents();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.recentEventsGridView.DataSource = eventEntries;
        }

        private void recentEventsGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (recentEventsGridView.SelectedRows.Count > 0)
            {
                var selectedRow = recentEventsGridView.SelectedRows[0];
                var entry = selectedRow.DataBoundItem as EventEntry;
                payloadTextBox.Text = entry!.Payload;
            }
        }
    }
}
