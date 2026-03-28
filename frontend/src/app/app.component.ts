import { Component } from '@angular/core';
import { AuthService } from './core/services/auth.service';

@Component({
  standalone: false,
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'frontend';

  constructor(public auth: AuthService) {}
}
