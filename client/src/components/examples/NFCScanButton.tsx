import NFCScanButton from '../NFCScanButton';

export default function NFCScanButtonExample() {
  return (
    <div className="p-8 flex justify-center">
      <NFCScanButton 
        onScan={(animalId) => console.log('Scanned animal:', animalId)}
      />
    </div>
  );
}