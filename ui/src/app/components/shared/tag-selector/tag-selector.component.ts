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
import { MatButtonModule } from '@angular/material/button';
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
    MatTooltipModule,
    MatButtonModule
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
  aliasControl = new FormControl('');
  filteredTags$!: Observable<Tag[]>;
  showCreateOption = false;
  searchTerm = '';
  
  // Track which tag is being aliased
  tagBeingAliased: RecipeTag | null = null;
  aliasError = '';

  tagCategories = [
    { value: TagCategory.Source, name: 'Source' },
    { value: TagCategory.Dietary, name: 'Dietary' },
    { value: TagCategory.Cuisine, name: 'Cuisine' },
    { value: TagCategory.MealType, name: 'Meal Type' },
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
          // Load system tags when search is empty
          return this.tagService.searchTags({ isGlobal: true, pageSize: 100 }).pipe(
            map(result => result.items)
          );
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
    
    // Show alias prompt for newly added tag if it's a system tag
    if (tag.isSystemTag || tag.isGlobal) {
      // Find the tag in selectedTags after it's been added
      setTimeout(() => {
        const addedTag = this.selectedTags.find(t => t.tagResourceId === tag.tagResourceId);
        if (addedTag && !addedTag.displayName && !addedTag.isOwnerAlias) {
          this.showAliasPrompt(addedTag);
        }
      }, 100);
    }
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

  showAliasPrompt(tag: RecipeTag) {
    this.tagBeingAliased = tag;
    this.aliasControl.setValue('');
    this.aliasError = '';
    // Focus the input after a short delay
    setTimeout(() => {
      const input = document.querySelector('.alias-input') as HTMLInputElement;
      if (input) input.focus();
    }, 50);
  }

  saveAlias() {
    if (!this.tagBeingAliased) return;
    
    const aliasValue = this.aliasControl.value?.trim();
    if (!aliasValue) {
      this.cancelAlias();
      return;
    }

    this.tagService.setAlias(this.tagBeingAliased.tagResourceId, {
      alias: aliasValue,
      showAliasPublicly: false
    }).subscribe({
      next: () => {
        // Update the tag display
        if (this.tagBeingAliased) {
          this.tagBeingAliased.displayName = aliasValue;
          this.tagBeingAliased.isOwnerAlias = true;
        }
        this.cancelAlias();
      },
      error: (err) => {
        this.aliasError = err.message || 'Failed to save alias';
        console.error('Error saving alias:', err);
      }
    });
  }

  cancelAlias() {
    this.tagBeingAliased = null;
    this.aliasControl.setValue('');
    this.aliasError = '';
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
    return tag ? (tag.alias || tag.name) : '';
  }

  getConfidencePercent(confidence?: number): number {
    return confidence ? Math.round(confidence * 100) : 0;
  }

  getTagsForCategory(tags: Tag[], category: number): Tag[] {
    return tags.filter(t => t.category === category);
  }

  getTagTooltip(tag: RecipeTag): string {
    if (tag.isOwnerAlias && tag.globalName) {
      return `Your name for: ${tag.globalName}`;
    }
    return tag.name;
  }

  getDisplayName(tag: RecipeTag): string {
    return tag.displayName || tag.name;
  }
}
