import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Observable, of, debounceTime, switchMap, map, startWith } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Tag, TagCategory, getCategoryName, RecipeTag, AssignTagItem } from '../../../models/tag.model';
import { TagService } from '../../../services/tag.service';

@Component({
  selector: 'app-tag-selector',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatChipsModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './tag-selector.component.html',
  styleUrls: ['./tag-selector.component.scss']
})
export class TagSelectorComponent implements OnInit {
  @Input() selectedTags: RecipeTag[] = [];
  @Input() readonly = false;
  @Output() tagsChanged = new EventEmitter<AssignTagItem[]>();
  @Output() tagRemoved = new EventEmitter<RecipeTag>();

  tagSearchControl = new FormControl('');
  filteredTags$!: Observable<Tag[]>;
  showCreateOption = false;
  searchTerm = '';

  tagCategories = [
    { value: TagCategory.Dietary, name: 'Dietary' },
    { value: TagCategory.Cuisine, name: 'Cuisine' },
    { value: TagCategory.MealType, name: 'Meal Type' },
    { value: TagCategory.Source, name: 'Source' },
    { value: TagCategory.Custom, name: 'Custom' }
  ];

  constructor(private tagService: TagService) {}

  ngOnInit() {
    this.filteredTags$ = this.tagSearchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap((value: string | null) => {
        this.searchTerm = value || '';
        if (!value || value.length < 2) {
          return of([]);
        }
        return this.tagService.searchTags({ name: value, pageSize: 20 }).pipe(
          map(result => {
            // Show "create new" option if no exact match
            this.showCreateOption = !result.items.some(
              t => t.name.toLowerCase() === value.toLowerCase()
            );
            return result.items;
          })
        );
      })
    );
  }

  onTagSelected(tag: Tag) {
    // Check if already selected
    if (this.selectedTags.some(t => t.tagResourceId === tag.tagResourceId)) {
      this.tagSearchControl.setValue('');
      return;
    }

    const assignTag: AssignTagItem = {
      tagResourceId: tag.tagResourceId
    };

    this.tagsChanged.emit([assignTag]);
    this.tagSearchControl.setValue('');
  }

  createNewTag(category: TagCategory) {
    const name = this.searchTerm.trim();
    if (!name) return;

    const assignTag: AssignTagItem = {
      name: name,
      category: category
    };

    this.tagsChanged.emit([assignTag]);
    this.tagSearchControl.setValue('');
    this.showCreateOption = false;
  }

  removeTag(tag: RecipeTag) {
    this.tagRemoved.emit(tag);
  }

  getTagCategoryName(category: number): string {
    return getCategoryName(category);
  }

  getTagColorClass(category: number): string {
    switch (category) {
      case TagCategory.Dietary:
        return 'tag-dietary';
      case TagCategory.Cuisine:
        return 'tag-cuisine';
      case TagCategory.MealType:
        return 'tag-mealtype';
      case TagCategory.Source:
        return 'tag-source';
      case TagCategory.Custom:
        return 'tag-custom';
      default:
        return '';
    }
  }

  displayFn(tag: Tag): string {
    return tag ? tag.name : '';
  }

  getConfidencePercent(confidence?: number): number {
    return confidence ? Math.round(confidence * 100) : 0;
  }

  getTagsForCategory(tags: Tag[], category: number): Tag[] {
    return tags.filter(t => t.category === category);
  }
}
