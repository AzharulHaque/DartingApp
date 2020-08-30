import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {

  @Output() cancelregister = new EventEmitter();

  model: any = {};

  constructor(private authservice: AuthService) { }

  ngOnInit() {}

  register(){
    this.authservice.register(this.model).subscribe(() => {
        console.log('registration successfull');
    }, error =>{
        console.log(error);
    });
  }

  cancel(){
    this.cancelregister.emit(false);
    console.log('Cancelled');
  }

}
