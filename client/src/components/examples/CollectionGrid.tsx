import CollectionGrid from '../CollectionGrid';
import type { Animal } from '../AnimalCard';
import lionImage from '@assets/generated_images/3D_cartoon_lion_7678e3f1.png';
import elephantImage from '@assets/generated_images/3D_cartoon_elephant_28b0801d.png';
import penguinImage from '@assets/generated_images/3D_cartoon_penguin_99368114.png';
import pandaImage from '@assets/generated_images/3D_cartoon_red_panda_5e73b17f.png';

const mockAnimals: Animal[] = [
  {
    id: 'lion-001',
    name: 'Savanna Lion',
    species: 'Panthera leo',
    habitat: 'African Grasslands',
    rarity: 'rare',
    imageUrl: lionImage,
    facts: ['Lions live in prides', 'Can roar 5 miles away'],
    collected: true
  },
  {
    id: 'elephant-002',
    name: 'African Elephant',
    species: 'Loxodonta africana',
    habitat: 'African Savannas',
    rarity: 'legendary',
    imageUrl: elephantImage,
    facts: ['Largest land mammal', 'Excellent memory'],
    collected: true
  },
  {
    id: 'penguin-003',
    name: 'Emperor Penguin',
    species: 'Aptenodytes forsteri',
    habitat: 'Antarctic Ice',
    rarity: 'common',
    imageUrl: penguinImage,
    facts: ['Can dive 500m deep', 'Males incubate eggs'],
    collected: false
  },
  {
    id: 'panda-004',
    name: 'Red Panda',
    species: 'Ailurus fulgens',
    habitat: 'Himalayan Forests',
    rarity: 'rare',
    imageUrl: pandaImage,
    facts: ['Also called firefox', 'Excellent climbers'],
    collected: false
  }
];

export default function CollectionGridExample() {
  return (
    <div className="p-4">
      <CollectionGrid 
        animals={mockAnimals}
        onAnimalClick={(animal) => console.log('Clicked animal:', animal.name)}
      />
    </div>
  );
}