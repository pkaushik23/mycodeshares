import { TestBed } from '@angular/core/testing';

import { AuthorizeGuardService } from './authorize-guard.service';

describe('AuthorizeGuardService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: AuthorizeGuardService = TestBed.get(AuthorizeGuardService);
    expect(service).toBeTruthy();
  });
});
