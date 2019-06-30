import { Component, Inject, ViewChild, ElementRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  agents: Agent[];
  agentsWithGardens: Agent[];

  isLoading: boolean;
  take: number = 10;
  category: string = 'koop';
  city: string = 'amsterdam';

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  loadAgents(): void {
    this.isLoading = true

    this.http.get<Agent[]>(this.baseUrl + `api/agent/getMostListings/${this.category}/${this.city}/${this.take}`).subscribe(
      result => this.agents = result,
      error => console.error(error),
      () => { this.isLoading = false }
    );
  }

  loadAgentsWithGardens(): void {
    this.isLoading = true

    this.http.get<Agent[]>(this.baseUrl + `api/agent/getMostGardenListings/${this.category}/${this.city}/${this.take}`).subscribe(
      result => this.agentsWithGardens = result,
      error => console.error(error),
      () => { this.isLoading = false }
    );
  }
}

interface Agent {
  name: string
  listings: number
  listingType: ListingType
}

enum ListingType {
  Garden,
  All
}
