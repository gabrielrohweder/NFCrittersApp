import AnimalCard from '../AnimalCard';
import type { Animal } from '../AnimalCard';
import lionImage from '@assets/generated_images/3D_cartoon_lion_7678e3f1.png';

const mockAnimal: Animal = {
  id: 'lion-001',
  name: 'Savanna Lion',
  species: 'Panthera leo',
  habitat: 'African Grasslands',
  rarity: 'rare',
  imageUrl: lionImage,
  facts: [
    'Lions are the only cats that live in groups called prides',
    'A lion\'s roar can be heard up to 5 miles away',
    'Lions sleep 16-20 hours per day'
  ],
  collected: true
};

export default function AnimalCardExample() {
  return (
    <div className="w-48">
      <AnimalCard 
        animal={mockAnimal} 
        onClick={() => console.log('Lion card clicked')}
      />
    </div>
  );
}