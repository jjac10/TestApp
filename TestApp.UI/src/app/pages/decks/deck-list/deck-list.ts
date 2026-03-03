import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { DeckService } from '../../../services/deck.service';
import { Deck } from '../../../models/models';

@Component({
  selector: 'app-deck-list',
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './deck-list.html',
  styleUrl: './deck-list.scss'
})
export class DeckListPage implements OnInit {
  decks = signal<Deck[]>([]);
  loading = signal(true);
  newDeckName = signal('');
  creating = signal(false);
  error = signal('');

  showDeleteModal = signal(false);
  deckToDelete = signal<Deck | null>(null);
  deleteConfirmName = signal('');
  deleting = signal(false);

  constructor(private deckService: DeckService) {}

  ngOnInit(): void {
    this.loadDecks();
  }

  loadDecks(): void {
    this.loading.set(true);
    this.deckService.getAll().subscribe({
      next: (decks) => {
        this.decks.set(decks);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Error al cargar las convocatorias');
        this.loading.set(false);
      }
    });
  }

  createDeck(): void {
    const name = this.newDeckName().trim();
    if (!name) return;

    const exists = this.decks().some(d => d.name.toLowerCase() === name.toLowerCase());
    if (exists) {
      this.error.set('Ya existe una convocatoria con ese nombre');
      setTimeout(() => this.error.set(''), 3000);
      return;
    }

    this.creating.set(true);
    this.deckService.create({ name }).subscribe({
      next: () => {
        this.newDeckName.set('');
        this.creating.set(false);
        this.loadDecks();
      },
      error: () => {
        this.error.set('Error al crear la convocatoria');
        this.creating.set(false);
      }
    });
  }

  openDeleteModal(deck: Deck, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.deckToDelete.set(deck);
    this.deleteConfirmName.set('');
    this.showDeleteModal.set(true);
  }

  confirmDelete(): void {
    const deck = this.deckToDelete();
    if (!deck) return;

    const totalFiles = deck.files?.length || 0;
    if (totalFiles > 0 && this.deleteConfirmName() !== deck.name) return;

    this.deleting.set(true);
    this.deckService.delete(deck.id).subscribe({
      next: () => {
        this.showDeleteModal.set(false);
        this.deckToDelete.set(null);
        this.deleting.set(false);
        this.loadDecks();
      },
      error: () => {
        this.deleting.set(false);
      }
    });
  }

  cancelDelete(): void {
    this.showDeleteModal.set(false);
    this.deckToDelete.set(null);
  }

  getTotalQuestions(deck: Deck): number {
    return deck.files?.reduce((sum, f) => sum + (f.questions?.length || 0), 0) || 0;
  }
}
