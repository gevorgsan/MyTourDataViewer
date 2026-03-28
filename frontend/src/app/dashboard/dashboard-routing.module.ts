import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SearchRequestComponent } from './search-request/search-request.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'search-requests', component: SearchRequestComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }

