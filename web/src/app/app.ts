import { Component, signal } from '@angular/core';

interface ServerInfo {
  machineName: string;
  hostName: string;
  containerHostname: string;
  environment: string;
  appVersion: string;
  utc: string;
}

interface RuntimeConfig {
  apiUrl?: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly info = signal<ServerInfo | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly loading = signal(false);
  protected readonly apiUrl = signal('');

  constructor() {
    void this.refresh();
  }

  protected async refresh(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const base = await this.resolveApiUrl();
      const response = await fetch(`${base}/api/info`);
      if (!response.ok) {
        throw new Error(`API returned HTTP ${response.status}`);
      }

      this.info.set((await response.json()) as ServerInfo);
    } catch (err) {
      this.info.set(null);
      this.error.set(err instanceof Error ? err.message : 'Request failed');
    } finally {
      this.loading.set(false);
    }
  }

  private async resolveApiUrl(): Promise<string> {
    const current = this.apiUrl();
    if (current) {
      return current;
    }

    const response = await fetch('/config.json', { cache: 'no-store' });
    if (!response.ok) {
      throw new Error(`Could not load /config.json (HTTP ${response.status})`);
    }

    const config = (await response.json()) as RuntimeConfig;
    const base = (config.apiUrl ?? '').replace(/\/$/, '');
    if (!base) {
      throw new Error('config.json is missing apiUrl');
    }

    this.apiUrl.set(base);
    return base;
  }
}
