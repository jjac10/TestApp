import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { StatisticsService } from '../../../services/statistics.service';
import { FileStatistics, ProgressHistory } from '../../../models/statistics';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-file-statistics',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './file-statistics.html',
  styleUrl: './file-statistics.scss'
})
export class FileStatisticsPage implements OnInit {
  deckId = 0;
  fileId = 0;
  stats = signal<FileStatistics | null>(null);
  progress = signal<ProgressHistory | null>(null);
  loading = signal(true);

  constructor(
    private route: ActivatedRoute,
    private statisticsService: StatisticsService
  ) {}

  ngOnInit(): void {
    this.deckId = Number(this.route.snapshot.paramMap.get('deckId'));
    this.fileId = Number(this.route.snapshot.paramMap.get('fileId'));
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.statisticsService.getFileStatistics(this.fileId).subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loadProgress();
      }
    });
  }

  loadProgress(): void {
    this.statisticsService.getFileProgress(this.fileId, 14).subscribe({
      next: (progress) => {
        this.progress.set(progress);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getMaxAttempts(): number {
    const p = this.progress();
    if (!p) return 1;
    return Math.max(1, ...p.dailyProgress.map(d => d.totalAttempts));
  }

  getBarHeight(attempts: number): number {
    return (attempts / this.getMaxAttempts()) * 120;
  }

  getTrendText(): string {
    const trend = this.progress()?.overallTrend || 0;
    if (trend > 5) return '📈 Mejorando';
    if (trend < -5) return '📉 Bajando';
    return '➡️ Estable';
  }

  getTrendClass(): string {
    const trend = this.progress()?.overallTrend || 0;
    if (trend > 5) return 'trend-up';
    if (trend < -5) return 'trend-down';
    return 'trend-stable';
  }
}
