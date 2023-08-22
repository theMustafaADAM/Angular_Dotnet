import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { shareReplay, map, tap, catchError } from 'rxjs/operators';
import { Country } from '../interfaces/country';
import { MessageService } from '../message.service';

@Injectable({
    providedIn: 'root'
})
export class CountryService {
    private countryListUrl: string = '/api/v1/country/getcountries';

    httpOptions = {
        headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    };

    private country$!: Observable<Country[]>;

    constructor(private http: HttpClient, private messageService: MessageService) {}

    // getCountries() {
    //   if (!this.country$) {
    //     this.country$ = this.http.get<any>(this.countryListUrl).pipe(
    //       shareReplay(),
    //       map((result) => {
    //         return result;
    //       })
    //     );
    //   }
    // // if countries cache exists return it
    // return this.country$;
    // }

    /** GET heroes from the server */
    getCountries(): Observable<Country[]> {
        return this.http.get<Country[]>(this.countryListUrl).pipe(
            tap((_) => this.log('fetched countries')),
            catchError(this.handleError<Country[]>('getcountries', []))
        );
    }

    /** GET Country by id. Return `undefined` when id not found */
    getHeroNo404<Data>(id: number): Observable<Country> {
        const url = `${this.countryListUrl}/?id=${id}`;
        return this.http.get<Country[]>(url).pipe(
            map((countr) => countr[0]), // returns a {0|1} element array
            tap((h) => {
                const outcome = h ? 'fetched' : 'did not find';
                this.log(`${outcome} hero id=${id}`);
            }),
            catchError(this.handleError<Country>(`getcountries id=${id}`))
        );
    }

    private handleError<T>(operation = 'operation', result?: T) {
        return (error: any): Observable<T> => {
            // TODO: send the error to remote logging infrastructure
            console.error(error); // log to console instead

            // TODO: better job of transforming error for user consumption
            this.log(`${operation} failed: ${error.message}`);

            // Let the app keep running by returning an empty result.
            return of(result as T);
        };
    }
    private log(message: string) {
        this.messageService.add(`HeroService: ${message}`);
    }
}
